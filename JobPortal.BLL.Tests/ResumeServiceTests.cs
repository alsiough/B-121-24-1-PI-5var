using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JobPortal.BLL.DTOs;
using JobPortal.BLL.Infrastructure;
using JobPortal.BLL.Services;
using JobPortal.DAL.Entities;
using JobPortal.DAL.Interfaces;
using Moq;
using NUnit.Framework;

namespace JobPortal.BLL.Tests
{
    [TestFixture]
    public class ResumeServiceTests
    {
        private Mock<IUnitOfWork> _uowMock;
        private Mock<IRepository<Resume>> _resumeRepoMock;
        private Mock<IRepository<User>> _userRepoMock;
        private Mock<IRepository<Vacancy>> _vacancyRepoMock;
        private ResumeService _service;

        [SetUp]
        public void Arrange()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _resumeRepoMock = new Mock<IRepository<Resume>>();
            _userRepoMock = new Mock<IRepository<User>>();
            _vacancyRepoMock = new Mock<IRepository<Vacancy>>();

            _uowMock.Setup(u => u.Resumes).Returns(_resumeRepoMock.Object);
            _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
            _uowMock.Setup(u => u.Vacancies).Returns(_vacancyRepoMock.Object);

            _service = new ResumeService(_uowMock.Object);
        }

        [Test]
        public void CreateResume_ValidEmployee_ShouldAddAndSave()
        {
            var dto = new ResumeDto { Title = "C# Developer", EmployeeId = 5, Skills = "C#" };
            var employee = new User { Id = 5, RoleId = RoleIds.Employee };
            _userRepoMock.Setup(r => r.Get(5)).Returns(employee);

            _service.CreateResume(dto);

            _resumeRepoMock.Verify(r => r.Create(It.Is<Resume>(x => x.Title == "C# Developer")), Times.Once);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void CreateResume_EmptyTitle_ShouldThrowValidationException()
        {
            var dto = new ResumeDto { Title = "", EmployeeId = 5 };

            Assert.Throws<ValidationException>(() => _service.CreateResume(dto));
        }

        [Test]
        public void CreateResume_WrongRole_ShouldThrowAccessDeniedException()
        {
            var dto = new ResumeDto { Title = "Dev", EmployeeId = 5 };
            var employer = new User { Id = 5, RoleId = RoleIds.Employer };
            _userRepoMock.Setup(r => r.Get(5)).Returns(employer);

            Assert.Throws<AccessDeniedException>(() => _service.CreateResume(dto));
        }

        [Test]
        public void GetResumes_NoFilters_ReturnsAllOrderedByDateDesc()
        {
            var resumes = new List<Resume>
            {
                new Resume { Id = 1, Title = "A", Skills = "", UpdatedAt = DateTime.UtcNow.AddDays(-2) },
                new Resume { Id = 2, Title = "B", Skills = "", UpdatedAt = DateTime.UtcNow }
            };
            _resumeRepoMock.Setup(r => r.GetAll()).Returns(resumes);

            var result = _service.GetResumes(null, null).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(2));
        }

        [Test]
        public void GetResumes_WithSearch_ReturnsOnlyMatching()
        {
            var resumes = new List<Resume>
            {
                new Resume { Id = 1, Title = "C# Dev",  Skills = "C#",   UpdatedAt = DateTime.UtcNow },
                new Resume { Id = 2, Title = "Java Dev", Skills = "Java", UpdatedAt = DateTime.UtcNow }
            };
            _resumeRepoMock.Setup(r => r.GetAll()).Returns(resumes);

            var result = _service.GetResumes("C#", null).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void GetResumesByVacancyRequirements_VacancyNotFound_ThrowsNotFoundException()
        {
            _vacancyRepoMock.Setup(r => r.Get(99)).Returns((Vacancy)null);

            Assert.Throws<NotFoundException>(() => _service.GetResumesByVacancyRequirements(99));
        }

        [Test]
        public void GetResumesByVacancyRequirements_MatchingSkills_ReturnsMatched()
        {
            var vacancy = new Vacancy { Id = 1, Requirements = "SQL, React" };
            _vacancyRepoMock.Setup(r => r.Get(1)).Returns(vacancy);
            _resumeRepoMock
                .Setup(r => r.GetAll())
                .Returns(new List<Resume> { new Resume { Id = 7, Title = "Full Stack", Skills = "SQL" } });

            var result = _service.GetResumesByVacancyRequirements(1).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(7));
        }

        [Test]
        public void UpdateResume_ValidEmployee_ShouldUpdateAndSave()
        {
            var existing = new Resume { Id = 2, Title = "Old", EmployeeId = 5 };
            var employee = new User { Id = 5, RoleId = RoleIds.Employee };
            _resumeRepoMock.Setup(r => r.Get(2)).Returns(existing);
            _userRepoMock.Setup(r => r.Get(5)).Returns(employee);

            _service.UpdateResume(new ResumeDto { Id = 2, Title = "Updated", EmployeeId = 5, Skills = "Go" });

            _resumeRepoMock.Verify(r => r.Update(It.Is<Resume>(x => x.Title == "Updated")), Times.Once);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void UpdateResume_NotFound_ThrowsNotFoundException()
        {
            _resumeRepoMock.Setup(r => r.Get(999)).Returns((Resume)null);

            Assert.Throws<NotFoundException>(() =>
                _service.UpdateResume(new ResumeDto { Id = 999, Title = "X", EmployeeId = 1 }));
        }

        [Test]
        public void DeleteResume_ValidEmployee_ShouldDeleteAndSave()
        {
            var existing = new Resume { Id = 2, EmployeeId = 5 };
            var employee = new User { Id = 5, RoleId = RoleIds.Employee };
            _resumeRepoMock.Setup(r => r.Get(2)).Returns(existing);
            _userRepoMock.Setup(r => r.Get(5)).Returns(employee);

            _service.DeleteResume(2, 5);

            _resumeRepoMock.Verify(r => r.Delete(2), Times.Once);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void DeleteResume_NotFound_ThrowsNotFoundException()
        {
            _resumeRepoMock.Setup(r => r.Get(999)).Returns((Resume)null);

            Assert.Throws<NotFoundException>(() => _service.DeleteResume(999, 5));
        }
    }
}
