using Microsoft.VisualStudio.TestTools.UnitTesting;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using static ERP.DEMO.Models.TestDb.Order;

namespace ERP.DEMO.Tests
{
    [TestClass]
    public class FilterByGenericTests
    {
        #region Modèles de test
        private class TestModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? NullableInt { get; set; }
            public decimal Price { get; set; }
            public decimal? NullablePrice { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreationDate { get; set; }
            public DateTime? NullableDate { get; set; }
            public OrderStatusList? Status { get; set; }
            public NestedModel Nested { get; set; }
        }

        private class NestedModel
        {
            public string City { get; set; }
        }
        #endregion

        #region Données et helpers
        private IQueryable<TestModel> GetTestData() => new List<TestModel>
        {
            new() { Id = 1, Name = "Alpha", Price = 10.5m,  IsActive = true,  NullableInt = 5,    CreationDate = DateTime.Today.AddDays(-5),  Status = OrderStatusList.Delivered,   Nested = new() { City = "Paris" } },
            new() { Id = 2, Name = "Beta",  Price = 50m,    IsActive = false, NullableInt = null, CreationDate = DateTime.Today.AddDays(-15), Status = OrderStatusList.Cancelled,   Nested = new() { City = "Lyon" } },
            new() { Id = 3, Name = "Gamma", Price = 200m,   IsActive = true,  NullableInt = 10,   CreationDate = DateTime.Today.AddDays(-2),  Status = null,                        Nested = new() { City = "Paris" } },
            new() { Id = 4, Name = null,    Price = 0m,     IsActive = false, NullableInt = null, CreationDate = DateTime.Today,              Status = OrderStatusList.Preparation, Nested = null },
        }.AsQueryable();

        private ICollection<IFilterDefinition<TestModel>> MakeFilter(
            string propertyName, string op, object value) =>
            new List<IFilterDefinition<TestModel>>
            {
                new FilterDefinition<TestModel>
                {
                    Id = Guid.NewGuid(),
                    Operator = op,
                    Value = value,
                    Title = propertyName
                }
            };
        #endregion

        #region STRING
        [TestMethod]
        public void String_Contains_ReturnsMatching()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Name", "contains", "alp"))
                .ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Alpha", result[0].Name);
        }

        [TestMethod]
        public void String_Contains_NullValue_DoesNotCrash()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Name", "contains", "Alpha"))
                .ToList();

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void String_Contains_EmptyFilter_ReturnsAll()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Name", "contains", ""))
                .ToList();

            Assert.AreEqual(4, result.Count);
        }

        [TestMethod]
        public void String_Equal_ReturnsMatching()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Name", "==", "Alpha"))
                .ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Alpha", result[0].Name);
        }

        [TestMethod]
        public void String_IsNull_ReturnsNullValues()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Name", "is null", null))
                .ToList();

            Assert.AreEqual(1, result.Count); // Id 4
            Assert.IsNull(result[0].Name);
        }

        [TestMethod]
        public void String_IsNotNull_ReturnsNonNullValues()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Name", "is not null", null))
                .ToList();

            Assert.AreEqual(3, result.Count);
        }
        #endregion

        #region INT
        [TestMethod]
        public void Int_Equal_ReturnsMatching()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Id", "==", 1))
                .ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].Id);
        }

        [TestMethod]
        public void Int_GreaterThan_ReturnsMatching()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Id", ">", 2))
                .ToList();

            Assert.AreEqual(2, result.Count); // Id 3 et 4
        }

        [TestMethod]
        public void Int_LessThanOrEqual_ReturnsMatching()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Id", "<=", 2))
                .ToList();

            Assert.AreEqual(2, result.Count); // Id 1 et 2
        }

        [TestMethod]
        public void NullableInt_IsNull_ReturnsNullValues()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("NullableInt", "is null", null))
                .ToList();

            Assert.AreEqual(2, result.Count); // Id 2 et 4
            Assert.IsTrue(result.All(x => x.NullableInt == null));
        }

        [TestMethod]
        public void NullableInt_IsNotNull_ReturnsNonNullValues()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("NullableInt", "is not null", null))
                .ToList();

            Assert.AreEqual(2, result.Count); // Id 1 et 3
            Assert.IsTrue(result.All(x => x.NullableInt != null));
        }

        [TestMethod]
        public void NullableInt_Equal_ReturnsMatching()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("NullableInt", "==", 5))
                .ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(5, result[0].NullableInt);
        }
        #endregion

        #region DECIMAL
        [TestMethod]
        public void Decimal_GreaterThanOrEqual_ReturnsMatching()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Price", ">=", 50m))
                .ToList();

            Assert.AreEqual(2, result.Count); // 50 et 200
        }

        [TestMethod]
        public void Decimal_Range_ReturnsMatching()
        {
            var filters = new List<IFilterDefinition<TestModel>>
            {
                new FilterDefinition<TestModel> { Operator = ">=", Value = 10m,  Title = "Price" },
                new FilterDefinition<TestModel> { Operator = "<=", Value = 100m, Title = "Price" },
            };

            var result = GetTestData().ApplyMudFilters(filters).ToList();

            Assert.AreEqual(2, result.Count); // 10.5 et 50
        }

        [TestMethod]
        public void Decimal_Equal_ReturnsMatching()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Price", "==", 50m))
                .ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(50m, result[0].Price);
        }

        [TestMethod]
        public void NullableDecimal_IsNull_ReturnsNullValues()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("NullablePrice", "is null", null))
                .ToList();

            Assert.AreEqual(4, result.Count); // toutes car NullablePrice jamais renseigné
        }
        #endregion

        #region BOOL
        [TestMethod]
        public void Bool_Equal_True_ReturnsActive()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("IsActive", "==", true))
                .ToList();

            Assert.AreEqual(2, result.Count); // Id 1 et 3
            Assert.IsTrue(result.All(x => x.IsActive));
        }

        [TestMethod]
        public void Bool_Equal_False_ReturnsInactive()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("IsActive", "==", false))
                .ToList();

            Assert.AreEqual(2, result.Count); // Id 2 et 4
            Assert.IsTrue(result.All(x => !x.IsActive));
        }
        #endregion

        #region DATETIME
        [TestMethod]
        public void DateTime_GreaterThanOrEqual_ReturnsMatching()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("CreationDate", ">=", DateTime.Today.AddDays(-5)))
                .ToList();

            Assert.AreEqual(3, result.Count); // -5 jours, -2 jours et aujourd'hui
        }

        [TestMethod]
        public void DateTime_Range_ReturnsMatching()
        {
            var filters = new List<IFilterDefinition<TestModel>>
            {
                new FilterDefinition<TestModel> { Operator = ">=", Value = DateTime.Today.AddDays(-10), Title = "CreationDate" },
                new FilterDefinition<TestModel> { Operator = "<",  Value = DateTime.Today,              Title = "CreationDate" },
            };

            var result = GetTestData().ApplyMudFilters(filters).ToList();

            Assert.AreEqual(2, result.Count); // -5 jours et -2 jours
        }

        [TestMethod]
        public void NullableDate_IsNull_ReturnsNullValues()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("NullableDate", "is null", null))
                .ToList();

            Assert.AreEqual(4, result.Count); // toutes car NullableDate jamais renseigné
        }

        [TestMethod]
        public void NullableDate_IsNotNull_ReturnsEmpty()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("NullableDate", "is not null", null))
                .ToList();

            Assert.AreEqual(0, result.Count);
        }
        #endregion

        #region ENUM
        [TestMethod]
        public void Enum_Equal_ReturnsMatching()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Status", "==", "Delivered"))
                .ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(OrderStatusList.Delivered, result[0].Status);
        }

        [TestMethod]
        public void Enum_MultiValue_ReturnsMatching()
        {
            var filters = new List<IFilterDefinition<TestModel>>
            {
                new FilterDefinition<TestModel> { Operator = "==", Value = "Delivered",   Title = "Status" },
                new FilterDefinition<TestModel> { Operator = "==", Value = "Cancelled",   Title = "Status" },
            };

            var result = GetTestData().ApplyMudFilters(filters).ToList();

            Assert.AreEqual(2, result.Count); // Id 1 et 2
        }

        [TestMethod]
        public void NullableEnum_IsNull_ReturnsNullValues()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Status", "is null", null))
                .ToList();

            Assert.AreEqual(1, result.Count); // Id 3
            Assert.IsNull(result[0].Status);
        }

        [TestMethod]
        public void NullableEnum_IsNotNull_ReturnsNonNullValues()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Status", "is not null", null))
                .ToList();

            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.All(x => x.Status != null));
        }
        #endregion

        #region PROPRIETES IMBRIQUEES
        [TestMethod]
        public void NestedProperty_Contains_ReturnsMatching()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Nested.City", "contains", "Paris"))
                .ToList();

            Assert.AreEqual(2, result.Count); // Id 1 et 3
        }

        [TestMethod]
        public void NestedProperty_Equal_ReturnsMatching()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Nested.City", "==", "Lyon"))
                .ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Lyon", result[0].Nested.City);
        }
        #endregion

        #region CAS LIMITES
        [TestMethod]
        public void EmptyFilters_ReturnsAll()
        {
            var result = GetTestData()
                .ApplyMudFilters(new List<IFilterDefinition<TestModel>>())
                .ToList();

            Assert.AreEqual(4, result.Count);
        }

        [TestMethod]
        public void NullFilters_ReturnsAll()
        {
            var result = GetTestData()
                .ApplyMudFilters(null)
                .ToList();

            Assert.AreEqual(4, result.Count);
        }

        [TestMethod]
        public void UnknownOperator_DoesNotCrash()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Name", "unknown_op", "test"))
                .ToList();

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void MultipleFilters_DifferentColumns_AppliesAndLogic()
        {
            var filters = new List<IFilterDefinition<TestModel>>
            {
                new FilterDefinition<TestModel> { Operator = "==",  Value = true,  Title = "IsActive" },
                new FilterDefinition<TestModel> { Operator = ">=",  Value = 100m,  Title = "Price" },
            };

            var result = GetTestData().ApplyMudFilters(filters).ToList();

            Assert.AreEqual(1, result.Count); // Id 3 : actif ET prix >= 100
            Assert.AreEqual(3, result[0].Id);
        }

        [TestMethod]
        public void FilterWithNullValue_DoesNotCrash()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Name", "==", null))
                .ToList();

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Filter_NotEqual_ReturnsNonMatching()
        {
            var result = GetTestData()
                .ApplyMudFilters(MakeFilter("Id", "!=", 1))
                .ToList();

            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.All(x => x.Id != 1));
        }
        #endregion
    }
}