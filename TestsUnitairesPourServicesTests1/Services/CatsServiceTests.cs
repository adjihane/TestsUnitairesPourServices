using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestsUnitairesPourServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestsUnitairesPourServices.Data;
using TestsUnitairesPourServices.Models;
using NuGet.Versioning;
using TestsUnitairesPourServices.Exceptions;

namespace TestsUnitairesPourServices.Services.Tests
{
    [TestClass()]
    public class CatsServiceTests
    {
        private DbContextOptions<ApplicationDBContext> _options;
        public CatsServiceTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDBContext>().UseInMemoryDatabase(databaseName: "CatsService")
                                                                            .UseLazyLoadingProxies(true)
                                                                            .Options;
        }

        [TestInitialize]
        public void Init()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            House[] houses =
            {
                new House{ Id=1,Address="123 rue1",OwnerName="Owner1" },
                new House{ Id=2,Address="456 rue2",OwnerName="Owner2" }
            };
            db.AddRange(houses);
            db.SaveChanges();
            Cat[] cats =
            {
                new Cat{Id=1,Name="Cat1",Age=1,House=null},
                new Cat{Id=2,Name="Cat2",Age=2,House=db.House.Find(1)}
            };
            db.AddRange(cats);
            db.SaveChanges();
        }

        [TestCleanup]
        public void Dispose()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            db.House.RemoveRange(db.House);
            db.Cat.RemoveRange(db.Cat);
            db.SaveChanges();
        }

        [TestMethod()]
        public void MoveTest()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService catService = new CatsService(db);

            var house1 = db.House.Find(1);
            var house2 = db.House.Find(2);

            var catAvecMaison = db.Cat.Find(2);

            var movedcat = catService.Move(catAvecMaison.Id, house1, house2);

            Assert.IsNotNull(movedcat, "le chat a pas bougé");

        }

        [TestMethod()]
        public void MoveTestCatIdNull()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService catService = new CatsService(db);

            var house1 = db.House.Find(1);
            var house2 = db.House.Find(2);

            //var cat = db.Cat.Find(444);

            //doit pas chercher le chat avant si non ca retourner null parce qu'il existe pas, il faut le tester direct dasn le assert.isnull
            var movedcat = catService.Move(444, house1, house2);

            Assert.IsNull(movedcat, "chat existe ");

        }

        [TestMethod()]
        public void MoveTestCatDoesntHaveAHouse()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService catService = new CatsService(db);

            var house1 = db.House.Find(1);
            var house2 = db.House.Find(2);

            var cat = db.Cat.Find(1);

            Exception e = Assert.ThrowsException<WildCatException>(() => catService.Move(cat.Id, house1, house2));
            Assert.AreEqual("On n'apprivoise pas les chats sauvages", e.Message);

        }

        [TestMethod()]
        public void MoveTestWrongFromHouse()
        {
            using ApplicationDBContext db = new ApplicationDBContext(_options);
            CatsService catService = new CatsService(db);

            var house1 = db.House.Find(1);
            var house2 = db.House.Find(2);

            var cat = db.Cat.Find(2);

            Exception e = Assert.ThrowsException<DontStealMyCatException>(() => catService.Move(cat.Id, house2, house1));
            Assert.AreEqual("Touche pas à mon chat!", e.Message);

        }
    }
}