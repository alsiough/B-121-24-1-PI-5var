using JobPortal.BLL.DTOs;
using JobPortal.BLL.Infrastructure;
using JobPortal.BLL.Services;
using JobPortal.DAL.Entities;
using JobPortal.DAL.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JobPortal.BLL.Tests
{
    [TestFixture]
    public class VacancyServiceTests
    {
        private Mock<IUnitOfWork> _uowMock;
        private Mock<IRepository<Vacancy>> _vacancyRepoMock;
        private Mock<IRepository<User>> _userRepoMock;
        private Mock<IRepository<Resume>> _resumeRepoMock;
        private VacancyService _service;

        [SetUp]
        public void Arrange()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _vacancyRepoMock = new Mock<IRepository<Vacancy>>();
            _userRepoMock = new Mock<IRepository<User>>();
            _resumeRepoMock = new Mock<IRepository<Resume>>();

            _uowMock.Setup(u => u.Vacancies).Returns(_vacancyRepoMock.Object);
            _uowMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
            _uowMock.Setup(u => u.Resumes).Returns(_resumeRepoMock.Object);

            _service = new VacancyService(_uowMock.Object);
        }

        [Test]
        public void CreateVacancy_ValidEmployer_ShouldAddAndSave()
        {
            var vacancyDto = new VacancyDto { Title = "Fullstack Developer", EmployerId = 10, Salary = 45000 };
            var employer = new User { Id = 10, RoleId = 2 };
            _userRepoMock.Setup(r => r.Get(10)).Returns(employer);

            _service.CreateVacancy(vacancyDto);

            _vacancyRepoMock.Verify(r => r.Create(It.Is<Vacancy>(v => v.Title == "Fullstack Developer")), Times.Once);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void CreateVacancy_InvalidRole_ShouldThrowAccessDeniedException()
        {
            var vacancyDto = new VacancyDto { Title = "DevOps", EmployerId = 11 };
            var wrongUser = new User { Id = 11, RoleId = 3 };
            _userRepoMock.Setup(r => r.Get(11)).Returns(wrongUser);

            Assert.Throws<AccessDeniedException>(delegate { _service.CreateVacancy(vacancyDto); });
        }

        [Test]
        public void CreateVacancy_EmptyTitle_ShouldThrowValidationException()
        {
            var dto = new VacancyDto { Title = "", EmployerId = 10 };

            Assert.Throws<ValidationException>(() => _service.CreateVacancy(dto));
        }

        [Test]
        public void GetVacancies_NoFilters_ReturnsAllOrderedByDateDesc()
        {
            var vacancies = new List<Vacancy>
            {
                new Vacancy { Id = 1, Title = "Dev", Salary = 1000, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Vacancy { Id = 2, Title = "QA",  Salary = 2000, CreatedAt = DateTime.UtcNow }
            };
            _vacancyRepoMock.Setup(r => r.GetAll()).Returns(vacancies);

            var result = _service.GetVacancies(null, null, null).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(2));
        }

        [Test]
        public void GetVacancies_WithSearch_ReturnsOnlyMatching()
        {
            var vacancies = new List<Vacancy>
            {
                new Vacancy { Id = 1, Title = "C# Developer", Description = "", CreatedAt = DateTime.UtcNow },
                new Vacancy { Id = 2, Title = "Java Developer", Description = "", CreatedAt = DateTime.UtcNow }
            };
            _vacancyRepoMock.Setup(r => r.GetAll()).Returns(vacancies);

            var result = _service.GetVacancies("C#", null, null).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("C# Developer"));
        }

        [Test]
        public void GetVacancies_WithMinSalary_ReturnsOnlyAboveThreshold()
        {
            var vacancies = new List<Vacancy>
            {
                new Vacancy { Id = 1, Title = "Junior", Salary = 500,  CreatedAt = DateTime.UtcNow },
                new Vacancy { Id = 2, Title = "Senior", Salary = 5000, CreatedAt = DateTime.UtcNow }
            };
            _vacancyRepoMock.Setup(r => r.GetAll()).Returns(vacancies);

            var result = _service.GetVacancies(null, null, 1000m).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Senior"));
        }

        [Test]
        public void GetVacanciesByResumeSkills_ResumeNotFound_ThrowsNotFoundException()
        {
            _resumeRepoMock.Setup(r => r.Get(99)).Returns((Resume)null);

            Assert.Throws<NotFoundException>(() => _service.GetVacanciesByResumeSkills(99));
        }

        [Test]
        public void GetVacanciesByResumeSkills_MatchingSkills_ReturnsMatched()
        {
            var resume = new Resume { Id = 1, Skills = "C#, SQL" };
            _resumeRepoMock.Setup(r => r.Get(1)).Returns(resume);
            _vacancyRepoMock
                .Setup(r => r.GetAll())
                .Returns(new List<Vacancy> { new Vacancy { Id = 5, Title = "Backend", Requirements = "C#" } });

            var result = _service.GetVacanciesByResumeSkills(1).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(5));
        }

        [Test]
        public void UpdateVacancy_ValidEmployer_ShouldUpdateAndSave()
        {
            var existing = new Vacancy { Id = 3, Title = "Old", EmployerId = 10 };
            var employer = new User { Id = 10, RoleId = 2 };
            _vacancyRepoMock.Setup(r => r.Get(3)).Returns(existing);
            _userRepoMock.Setup(r => r.Get(10)).Returns(employer);

            _service.UpdateVacancy(new VacancyDto { Id = 3, Title = "New Title", EmployerId = 10, Salary = 2000 });

            _vacancyRepoMock.Verify(r => r.Update(It.Is<Vacancy>(v => v.Title == "New Title")), Times.Once);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void UpdateVacancy_NotFound_ThrowsNotFoundException()
        {
            _vacancyRepoMock.Setup(r => r.Get(999)).Returns((Vacancy)null);

            Assert.Throws<NotFoundException>(() =>
                _service.UpdateVacancy(new VacancyDto { Id = 999, Title = "X", EmployerId = 1 }));
        }

        [Test]
        public void UpdateVacancy_WrongOwner_ThrowsAccessDeniedException()
        {
            var existing = new Vacancy { Id = 3, EmployerId = 10 };
            var otherUser = new User { Id = 20, RoleId = 2 };
            _vacancyRepoMock.Setup(r => r.Get(3)).Returns(existing);
            _userRepoMock.Setup(r => r.Get(20)).Returns(otherUser);

            Assert.Throws<AccessDeniedException>(() =>
                _service.UpdateVacancy(new VacancyDto { Id = 3, Title = "X", EmployerId = 20 }));
        }

        [Test]
        public void DeleteVacancy_ValidEmployer_ShouldDeleteAndSave()
        {
            var existing = new Vacancy { Id = 3, EmployerId = 10 };
            var employer = new User { Id = 10, RoleId = 2 };
            _vacancyRepoMock.Setup(r => r.Get(3)).Returns(existing);
            _userRepoMock.Setup(r => r.Get(10)).Returns(employer);

            _service.DeleteVacancy(3, 10);

            _vacancyRepoMock.Verify(r => r.Delete(3), Times.Once);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void DeleteVacancy_NotFound_ThrowsNotFoundException()
        {
            _vacancyRepoMock.Setup(r => r.Get(999)).Returns((Vacancy)null);

            Assert.Throws<NotFoundException>(() => _service.DeleteVacancy(999, 10));
        }
    }
}
