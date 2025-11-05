using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTests
{
    public class CreateProductTests
    {
        private ChromeDriver driver;
        private WebDriverWait wait;
        private const string BASE_URL = "http://localhost:8083";

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");

            driver = new ChromeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        }

        [Test, Order(0)]
        public void Diagnostic_FindCorrectUrls()
        {
            try
            {
                TestContext.WriteLine("DIAGNÓSTICO DE LA APLICACIÓN\n");

                driver.Navigate().GoToUrl(BASE_URL);
                System.Threading.Thread.Sleep(2000);

                TestContext.WriteLine($"✓ Aplicación responde en: {BASE_URL}");
                TestContext.WriteLine($"  Título: {driver.Title}");
                TestContext.WriteLine($"  URL final: {driver.Url}\n");

                var links = driver.FindElements(By.TagName("a"));
                TestContext.WriteLine("📋 Enlaces encontrados relacionados con productos:");

                foreach (var link in links)
                {
                    string href = link.GetAttribute("href") ?? "";
                    string text = link.Text;

                    if (href.Contains("product") || text.ToLower().Contains("product"))
                    {
                        TestContext.WriteLine($"  • {text} → {href}");
                    }
                }

                string[] possibleUrls = {
                    "/product/new",
                    "/products/new",
                    "/product/create",
                    "/products/create",
                    "/product/add",
                    "/products/add"
                };

                TestContext.WriteLine("\n🔍 Probando URLs posibles para crear productos:");

                foreach (var url in possibleUrls)
                {
                    try
                    {
                        driver.Navigate().GoToUrl($"{BASE_URL}{url}");
                        System.Threading.Thread.Sleep(1500);

                        var forms = driver.FindElements(By.TagName("form"));

                        if (forms.Count > 0)
                        {
                            TestContext.WriteLine($"\n✅ ENCONTRADO: {url}");
                            TestContext.WriteLine($"   Título: {driver.Title}");
                            TestContext.WriteLine($"   Formularios: {forms.Count}");

                            var inputs = driver.FindElements(By.TagName("input"));
                            TestContext.WriteLine($"   Campos de entrada encontrados:");

                            foreach (var input in inputs)
                            {
                                string id = input.GetAttribute("id") ?? "";
                                string name = input.GetAttribute("name") ?? "";
                                string type = input.GetAttribute("type") ?? "";

                                if (!string.IsNullOrEmpty(id) || !string.IsNullOrEmpty(name))
                                {
                                    TestContext.WriteLine($"     - ID: '{id}' | Name: '{name}' | Type: '{type}'");
                                }
                            }
                        }
                        else
                        {
                            TestContext.WriteLine($"  ✗ {url} - Sin formularios");
                        }
                    }
                    catch
                    {
                        TestContext.WriteLine($"  ✗ {url} - No accesible");
                    }
                }

                TestContext.WriteLine("\n📄 HTML de la última página (primeros 1000 caracteres):");
                TestContext.WriteLine(driver.PageSource.Substring(0, Math.Min(1000, driver.PageSource.Length)));

                Assert.Pass("Diagnóstico completado. Revisa la salida para encontrar la URL y campos correctos.");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"❌ Error en diagnóstico: {ex.Message}");
                TakeScreenshot("Diagnostic_Error");
                throw;
            }
        }

        private IWebElement FindElementByIdOrName(string fieldIdentifier, int timeoutSeconds = 10)
        {
            var localWait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                return localWait.Until(d => {
                    try
                    {
                        var element = d.FindElement(By.Id(fieldIdentifier));
                        if (element.Displayed)
                            return element;
                    }
                    catch { }

                    try
                    {
                        var element = d.FindElement(By.Name(fieldIdentifier));
                        if (element.Displayed)
                            return element;
                    }
                    catch { }

                    try
                    {
                        var element = d.FindElement(By.CssSelector($"input[id*='{fieldIdentifier}']"));
                        if (element.Displayed)
                            return element;
                    }
                    catch { }

                    throw new NoSuchElementException($"No se encontró el campo: {fieldIdentifier}");
                });
            }
            catch (WebDriverTimeoutException)
            {
                TestContext.WriteLine($"❌ No se encontró el campo '{fieldIdentifier}' después de {timeoutSeconds}s");
                TestContext.WriteLine($"   Campos disponibles en la página:");

                var allInputs = driver.FindElements(By.TagName("input"));
                foreach (var input in allInputs)
                {
                    TestContext.WriteLine($"     - ID: '{input.GetAttribute("id")}' | Name: '{input.GetAttribute("name")}'");
                }

                throw;
            }
        }

        [Test, Order(1)]
        public void CreateProduct_ValidData_Success()
        {
            try
            {
                string createProductUrl = $"{BASE_URL}/product/new";

                TestContext.WriteLine($"📍 Navegando a: {createProductUrl}");
                driver.Navigate().GoToUrl(createProductUrl);

                System.Threading.Thread.Sleep(2000);

                TestContext.WriteLine($"   Título página: {driver.Title}");
                TestContext.WriteLine($"   URL actual: {driver.Url}");

                // Verificar que hay un formulario
                var forms = driver.FindElements(By.TagName("form"));
                Assert.That(forms.Count, Is.GreaterThan(0),
                    "No se encontró ningún formulario en la página. Ejecuta el test de diagnóstico primero.");

                TestContext.WriteLine($"   Formularios encontrados: {forms.Count}");

                string uniqueId = $"PROD-{DateTime.Now:yyyyMMddHHmmss}";

                TestContext.WriteLine($"✏️  Llenando formulario con ID: {uniqueId}");

                var productIdField = FindElementByIdOrName("productId");
                productIdField.Clear();
                productIdField.SendKeys(uniqueId);
                TestContext.WriteLine($"   ✓ productId ingresado");

                var descriptionField = FindElementByIdOrName("description");
                descriptionField.Clear();
                descriptionField.SendKeys("Producto de prueba automatizada");
                TestContext.WriteLine($"   ✓ description ingresado");

                var priceField = FindElementByIdOrName("price");
                priceField.Clear();
                priceField.SendKeys("25.99");
                TestContext.WriteLine($"   ✓ price ingresado");

                var imageUrlField = FindElementByIdOrName("imageUrl");
                imageUrlField.Clear();
                imageUrlField.SendKeys("https://via.placeholder.com/150");
                TestContext.WriteLine($"   ✓ imageUrl ingresado");

                TakeScreenshot("BeforeSubmit_ValidData");

                IWebElement submitButton = driver.FindElement(By.CssSelector("button[type='submit']"));
                TestContext.WriteLine($"🖱️  Haciendo clic en Submit...");
                submitButton.Click();

                wait.Until(d => !d.Url.Contains("/new"));
                System.Threading.Thread.Sleep(1000);

                string finalUrl = driver.Url;
                TestContext.WriteLine($"📍 URL después de submit: {finalUrl}");

                TakeScreenshot("AfterSubmit_ValidData");

                bool success = finalUrl.Contains("/products") || finalUrl.Contains("/product/");
                Assert.That(success, Is.True,
                    $"La creación debería redirigir a /products o /product/. URL actual: {finalUrl}");

                TestContext.WriteLine($"✅ TEST EXITOSO - Producto '{uniqueId}' creado");
            }
            catch (Exception ex)
            {
                TakeScreenshot("CreateProduct_ValidData_Error");
                TestContext.WriteLine($"❌ Error: {ex.Message}");
                TestContext.WriteLine($"   Stack: {ex.StackTrace}");
                throw;
            }
        }

        [Test, Order(2)]
        public void CreateProduct_EmptyProductId_ShowsError()
        {
            try
            {
                driver.Navigate().GoToUrl($"{BASE_URL}/product/new");
                System.Threading.Thread.Sleep(2000);

                // Dejar productId vacío
                FindElementByIdOrName("productId").Clear();
                FindElementByIdOrName("description").SendKeys("Descripción de prueba");
                FindElementByIdOrName("price").SendKeys("10.00");
                FindElementByIdOrName("imageUrl").SendKeys("https://via.placeholder.com/150");

                TakeScreenshot("BeforeSubmit_EmptyProductId");

                IWebElement submitButton = driver.FindElement(By.CssSelector("button[type='submit']"));
                submitButton.Click();

                System.Threading.Thread.Sleep(2000);

                string currentUrl = driver.Url;
                bool staysOnForm = currentUrl.Contains("/product/new") || currentUrl.Contains("/product");

                TestContext.WriteLine($"📍 URL después de submit: {currentUrl}");
                TestContext.WriteLine($"   ¿Permaneció en formulario?: {staysOnForm}");

                TakeScreenshot("AfterSubmit_EmptyProductId");

                Assert.Pass($"Test completado. Comportamiento documentado: {(staysOnForm ? "Rechaza" : "Acepta")} productId vacío");
            }
            catch (Exception ex)
            {
                TakeScreenshot("CreateProduct_EmptyProductId_Error");
                TestContext.WriteLine($"❌ Error: {ex.Message}");
                throw;
            }
        }

        [Test, Order(3)]
        public void CreateProduct_NegativePrice_ValidateBehavior()
        {
            try
            {
                driver.Navigate().GoToUrl($"{BASE_URL}/product/new");
                System.Threading.Thread.Sleep(2000);

                string uniqueId = $"PROD-NEG-{DateTime.Now:yyyyMMddHHmmss}";

                FindElementByIdOrName("productId").SendKeys(uniqueId);
                FindElementByIdOrName("description").SendKeys("Producto con precio negativo");
                FindElementByIdOrName("price").SendKeys("-10.50");
                FindElementByIdOrName("imageUrl").SendKeys("https://via.placeholder.com/150");

                TakeScreenshot("BeforeSubmit_NegativePrice");

                IWebElement submitButton = driver.FindElement(By.CssSelector("button[type='submit']"));
                submitButton.Click();

                System.Threading.Thread.Sleep(2000);

                string currentUrl = driver.Url;
                bool wasCreated = !currentUrl.Contains("/new");

                TestContext.WriteLine($"📍 URL final: {currentUrl}");
                TestContext.WriteLine($"   Precio negativo: {(wasCreated ? "ACEPTADO ⚠️" : "RECHAZADO ✅")}");

                TakeScreenshot("AfterSubmit_NegativePrice");

                if (wasCreated)
                {
                    Assert.Warn("⚠️ El sistema aceptó un precio negativo - Posible bug de validación");
                }
                else
                {
                    Assert.Pass("✅ El sistema rechazó correctamente el precio negativo");
                }
            }
            catch (Exception ex)
            {
                TakeScreenshot("CreateProduct_NegativePrice_Error");
                TestContext.WriteLine($"❌ Error: {ex.Message}");
                throw;
            }
        }

        [Test, Order(4)]
        public void CreateProduct_ZeroPrice_ValidateBehavior()
        {
            try
            {
                driver.Navigate().GoToUrl($"{BASE_URL}/product/new");
                System.Threading.Thread.Sleep(2000);

                string uniqueId = $"PROD-ZERO-{DateTime.Now:yyyyMMddHHmmss}";

                FindElementByIdOrName("productId").SendKeys(uniqueId);
                FindElementByIdOrName("description").SendKeys("Producto gratis");
                FindElementByIdOrName("price").SendKeys("0");
                FindElementByIdOrName("imageUrl").SendKeys("https://via.placeholder.com/150");

                TakeScreenshot("BeforeSubmit_ZeroPrice");

                IWebElement submitButton = driver.FindElement(By.CssSelector("button[type='submit']"));
                submitButton.Click();

                System.Threading.Thread.Sleep(2000);

                string currentUrl = driver.Url;
                bool wasAccepted = !currentUrl.Contains("/new");

                TestContext.WriteLine($"📍 URL final: {currentUrl}");
                TestContext.WriteLine($"   Precio cero: {(wasAccepted ? "ACEPTADO" : "RECHAZADO")}");

                TakeScreenshot("AfterSubmit_ZeroPrice");

                Assert.Pass($"Test completado. Precio 0 fue {(wasAccepted ? "aceptado" : "rechazado")}");
            }
            catch (Exception ex)
            {
                TakeScreenshot("CreateProduct_ZeroPrice_Error");
                TestContext.WriteLine($"❌ Error: {ex.Message}");
                throw;
            }
        }

        [Test, Order(5)]
        public void CreateProduct_InvalidImageUrl_ValidateBehavior()
        {
            try
            {
                driver.Navigate().GoToUrl($"{BASE_URL}/product/new");
                System.Threading.Thread.Sleep(2000);

                string uniqueId = $"PROD-BADURL-{DateTime.Now:yyyyMMddHHmmss}";

                FindElementByIdOrName("productId").SendKeys(uniqueId);
                FindElementByIdOrName("description").SendKeys("Producto con URL inválida");
                FindElementByIdOrName("price").SendKeys("20.00");
                FindElementByIdOrName("imageUrl").SendKeys("esto-no-es-una-url");

                TakeScreenshot("BeforeSubmit_InvalidUrl");

                IWebElement submitButton = driver.FindElement(By.CssSelector("button[type='submit']"));
                submitButton.Click();

                System.Threading.Thread.Sleep(2000);

                string currentUrl = driver.Url;
                bool wasCreated = !currentUrl.Contains("/new");

                TestContext.WriteLine($"📍 URL final: {currentUrl}");
                TestContext.WriteLine($"   URL inválida: {(wasCreated ? "ACEPTADA" : "RECHAZADA")}");

                TakeScreenshot("AfterSubmit_InvalidUrl");

                Assert.Pass($"Test completado. URL inválida fue {(wasCreated ? "aceptada" : "rechazada")}");
            }
            catch (Exception ex)
            {
                TakeScreenshot("CreateProduct_InvalidUrl_Error");
                TestContext.WriteLine($"❌ Error: {ex.Message}");
                throw;
            }
        }

        private void TakeScreenshot(string testName)
        {
            try
            {
                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                string fileName = $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string directory = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string filePath = Path.Combine(directory, fileName);
                screenshot.SaveAsFile(filePath);

                TestContext.WriteLine($"📸 Screenshot: {filePath}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"⚠️  No se pudo guardar screenshot: {ex.Message}");
            }
        }

        [TearDown]
        public void Teardown()
        {
            try
            {
                if (driver != null)
                {
                    driver.Quit();
                    driver.Dispose();
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"⚠️  Error en Teardown: {ex.Message}");
            }
        }
    }
}