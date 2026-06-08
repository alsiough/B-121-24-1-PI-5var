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
    public class JobApplicationServiceTests
    {
        private Mock<IUnitOfWork> _uowMock;
        private Mock<IRepository<JobApplication>> _appRepoMock;
        private Mock<IRepository<Resume>> _resumeRepoMock;
        private Mock<IRepository<Vacancy>> _vacancyRepoMock;
        private JobApplicationService _service;

        [SetUp]
        public void Arrange()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _appRepoMock = new Mock<IRepository<JobApplication>>();
            _resumeRepoMock = new Mock<IRepository<Resume>>();
            _vacancyRepoMock = new Mock<IRepository<Vacancy>>();

            _uowMock.Setup(u => u.JobApplications).Returns(_appRepoMock.Object);
            _uowMock.Setup(u => u.Resumes).Returns(_resumeRepoMock.Object);
            _uowMock.Setup(u => u.Vacancies).Returns(_vacancyRepoMock.Object);

            _service = new JobApplicationService(_uowMock.Object);
        }

        [Test]
        public void ApplyForVacancy_EmployeeInitiator_CreatesWithAppliedStatus()
        {
            _resumeRepoMock.Setup(r => r.Get(1)).Returns(new Resume { Id = 1 });
            _vacancyRepoMock.Setup(r => r.Get(2)).Returns(new Vacancy { Id = 2 });

            _service.ApplyForVacancy(1, 2, "Employee");

            _appRepoMock.Verify(r => r.Create(It.Is<JobApplication>(a =>
                a.ResumeId == 1 && a.VacancyId == 2 && a.Status == "Applied")), Times.Once);
            _uowMock.Verify(u => u.Save(), Times.Once);
        }

        [Test]
        public void ApplyForVacancy_EmployerInitiator_CreatesWithOfferedStatus()
        {
            _resumeRepoMock.Setup(r => r.Get(1)).Returns(new Resume { Id = 1 });
            _vacancyRepoMock.Setup(r => r.Get(2)).Returns(new Vacancy { Id = 2 });

            _service.ApplyForVacancy(1, 2, "Employer");

            _appRepoMock.Verify(r => r.Create(It.Is<JobApplication>(a =>
                a.Status == "Offered")), Times.Once);
        }

        [Test]
        public void ApplyForVacancy_ResumeNotFound_ThrowsNotFoundException()
        {
            _resumeRepoMock.Setup(r => r.Get(99)).Returns((Resume)null);
            _vacancyRepoMock.Setup(r => r.Get(2)).Returns(new Vacancy { Id = 2 });

            Assert.Throws<NotFoundException>(() => _service.ApplyForVacancy(99, 2, "Employee"));
        }

        [Test]
        public void ApplyForVacancy_VacancyNotFound_ThrowsNotFoundException()
        {
            _resumeRepoMock.Setup(r => r.Get(1)).Returns(new Resume { Id = 1 });
            _vacancyRepoMock.Setup(r => r.Get(99)).Returns((Vacancy)null);

            Assert.Throws<NotFoundException>(() => _service.ApplyForVacancy(1, 99, "Employee"));
        }

        [Test]
        public void GetApplicationsForVacancy_ReturnsMatchingApplications()
        {
            var apps = new List<JobApplication>
            {
                new JobApplication
                {
                    Id = 1, VacancyId = 5, ResumeId = 3, Status = "Applied",
                    ActionDate = DateTime.UtcNow,
                    Vacancy = new Vacancy { Title = "Dev" },
                    Resume  = new Resume  { Title = "My CV" }
                }
            };
            _appRepoMock
                .Setup(r => r.FindWithIncludes(
                    It.IsAny<Expression<Func<JobApplication, bool>>>(),
                    It.IsAny<Expression<Func<JobApplication, object>>[]>()))
                .Returns(apps);

            var result = _service.GetApplicationsForVacancy(5).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].VacancyTitle, Is.EqualTo("Dev"));
            Assert.That(result[0].ResumeTitle,  Is.EqualTo("My CV"));
            Assert.That(result[0].Status, Is.EqualTo("Applied"));
        }

        [Test]
        public void GetApplicationsForVacancy_NoApplications_ReturnsEmptyList()
        {
            _appRepoMock
                .Setup(r => r.FindWithIncludes(
                    It.IsAny<Expression<Func<JobApplication, bool>>>(),
                    It.IsAny<Expression<Func<JobApplication, object>>[]>()))
                .Returns(new List<JobApplication>());

            var result = _service.GetApplicationsForVacancy(5).ToList();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetApplicationsForResume_ReturnsMatchingApplications()
        {
            var apps = new List<JobApplication>
            {
                new JobApplication
                {
                    Id = 2, VacancyId = 7, ResumeId = 3, Status = "Offered",
                    ActionDate = DateTime.UtcNow,
                    Vacancy = new Vacancy { Title = "PM" },
                    Resume  = new Resume  { Title = "My CV" }
                }
            };
            _appRepoMock
                .Setup(r => r.FindWithIncludes(
                    It.IsAny<Expression<Func<JobApplication, bool>>>(),
                    It.IsAny<Expression<Func<JobApplication, object>>[]>()))
                .Returns(apps);

            var result = _service.GetApplicationsForResume(3).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Status, Is.EqualTo("Offered"));
        }
    }
}
