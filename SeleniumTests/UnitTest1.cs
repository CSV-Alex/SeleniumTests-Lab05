using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SeleniumTests
{
    public class CreateProductTests
    {
        private IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);
        }

        [Test]
        public void CreateProduct_ValidData_Success()
        {
            // Arrange
            driver.Navigate().GoToUrl("http://localhost:8083/new");

            // Act
            driver.FindElement(By.Name("name")).SendKeys("Prod A");
            driver.FindElement(By.Name("price")).SendKeys("12.50");
            driver.FindElement(By.Name("description")).SendKeys("desc");
            driver.FindElement(By.Name("imageUrl")).SendKeys("http://valid.url/image.jpg");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Assert
            string currentUrl = driver.Url;
            Thread.Sleep(5000);
            Assert.That(currentUrl, Does.Contain("/product/") | Does.Contain("/products"));
        }

        [Test]
        public void CreateProduct_EmptyName_ShowsError()
        {
            driver.Navigate().GoToUrl("http://localhost:8083/new");

            driver.FindElement(By.Name("name")).SendKeys("");
            driver.FindElement(By.Name("price")).SendKeys("10");
            driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            var errorMessage = driver.FindElement(By.CssSelector(".error, .alert"));
            Assert.That(errorMessage.Text, Does.Contain("required").IgnoreCase);
        }

        [TearDown]
        public void Teardown()
        {
            driver?.Quit();
        }
    }
}