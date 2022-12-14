using ChainingAssertion;
using DotNetCoreWebSample.Web.Models;
using DotNetCoreWebSample.Web.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace DotNetCoreWebSample.Web.Test.Repositories
{
    public class TodoRepositoryTest
    {
        private readonly TodoRepository _repository;
        private readonly IQueryable<Todo> _mockData;
        private readonly Mock<DbSet<Todo>> _mockDbSet;

        public TodoRepositoryTest()
        {
            _mockData = new List<Todo>
            {
                new Todo { Id = 1, Description = "Title 001", CreatedDate = DateTime.Now },
                new Todo { Id = 2, Description = "Title 002", CreatedDate = DateTime.Now },
                new Todo { Id = 3, Description = "Title 003", CreatedDate = DateTime.Now }
            }.AsQueryable();

            _mockDbSet = new Mock<DbSet<Todo>>();
            _mockDbSet.As<IQueryable<Todo>>().Setup(m => m.Provider).Returns(_mockData.Provider);
            _mockDbSet.As<IQueryable<Todo>>().Setup(m => m.Expression).Returns(_mockData.Expression);
            _mockDbSet.As<IQueryable<Todo>>().Setup(m => m.ElementType).Returns(_mockData.ElementType);
            _mockDbSet.As<IQueryable<Todo>>().Setup(m => m.GetEnumerator()).Returns(_mockData.GetEnumerator());
            _mockDbSet.Setup(_ => _.Find(It.IsAny<object[]>()))
                .Returns<object[]>(ids => _mockData.FirstOrDefault(_ => _.Id == (int) ids[0]));

            var mockContext = new Mock<DotnetCoreWebSampleContext>();
            mockContext.Setup(_ => _.Todo).Returns(_mockDbSet.Object);
            mockContext.Setup(_ => _.Set<Todo>()).Returns(_mockDbSet.Object);
            _repository = new TodoRepository(mockContext.Object);
        }

        [Fact]
        public void Get_正常系()
        {
            // Arrange・Act
            var result = _repository.Get();

            // Assert
            result.Count.Is(3);

            var expect = _mockData.ToList();
            foreach (var r in result)
            {
                expect.First(_ => _.Id == r.Id).IsStructuralEqual(r);
            }
        }

        [Fact]
        public void Get_条件有_正常系()
        {
            // Arrange
            const int id = 1;
            Expression<Func<Todo, bool>> condition = t => t.Id == id;

            // Act
            var result = _repository.Get(condition);

            // Assert
            result.Count.Is(1);
            result.First().IsStructuralEqual(_mockData.First(_ => _.Id == id));
        }

        [Fact]
        public void Add_正常系()
        {
            // Arrange
            var expect = new Todo { Id = 4, Description = "Title 004", CreatedDate = DateTime.Now };

            // Act
            _repository.Add(expect);

            // Assert
            _mockDbSet.Verify(_ => _.Add(It.Is<Todo>(t => t.Id == expect.Id
                                                            && t.Description == expect.Description
                                                            && t.CreatedDate == expect.CreatedDate)), Times.Once);
        }

        [Fact]
        public void Find_正常系()
        {
            // Arrange
            const int id = 1;

            // Act
            var result = _repository.Find(id);

            // Assert
            result.IsStructuralEqual(_mockData.First(_ => _.Id == id));
        }

    }
}
