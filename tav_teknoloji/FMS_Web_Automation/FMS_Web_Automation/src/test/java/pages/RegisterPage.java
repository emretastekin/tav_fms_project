package pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.interactions.Actions;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

public class RegisterPage {
    WebDriver driver;
    WebDriverWait wait;

    // Locator'lar
    private By emailInputLoc = By.xpath("//input[@type='email']");
    private By passwordInputLoc = By.xpath("//input[@type='password']");

    // Görseldeki yapıya göre: type="submit" ve class="primary-btn" olan Kayıt Ol butonu
    private By registerButtonLoc = By.cssSelector("button[type='submit'].primary-btn");

    // Login sayfasındaki "Kayıt Olun" butonu
    private By goToRegisterButtonLoc = By.cssSelector("button.text-btn");

    public RegisterPage(WebDriver driver, WebDriverWait wait) {
        this.driver = driver;
        this.wait = wait;
    }

    public void goToRegisterPageViaLogin() {
        driver.get("http://localhost:5173/login");

        // Butonun tıklanabilir olmasını bekle ve tıkla
        WebElement registerButton = wait.until(ExpectedConditions.elementToBeClickable(goToRegisterButtonLoc));

        new Actions(driver)
                .moveToElement(registerButton)
                .click()
                .perform();
    }

    public void register(String email, String password) {
        WebElement emailInput = wait.until(ExpectedConditions.visibilityOfElementLocated(emailInputLoc));
        WebElement passwordInput = driver.findElement(passwordInputLoc);

        emailInput.clear();
        emailInput.sendKeys(email);
        passwordInput.clear();
        passwordInput.sendKeys(password);

        // Kayıt ol butonunun tıklanabilir olmasını bekle
        WebElement registerButton = wait.until(ExpectedConditions.elementToBeClickable(registerButtonLoc));

        // Actions ile güvenli tıklama
        new Actions(driver)
                .moveToElement(registerButton)
                .click()
                .perform();
    }
}