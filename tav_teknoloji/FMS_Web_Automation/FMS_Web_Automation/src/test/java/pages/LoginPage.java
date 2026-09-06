package pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.interactions.Actions;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

public class LoginPage {
    WebDriver driver;
    WebDriverWait wait;

    // Locator'lar (Element tanımları)
    private By emailInputLoc = By.xpath("//input[@type='email']");
    private By passwordInputLoc = By.xpath("//input[@type='password']");
    private By loginButtonLoc = By.cssSelector("button.primary-btn");

    public LoginPage(WebDriver driver, WebDriverWait wait) {
        this.driver = driver;
        this.wait = wait;
    }

    public void goToLoginPage() {
        driver.get("http://localhost:5173/login");
    }

    public void login(String email, String password) {
        WebElement emailInput = wait.until(ExpectedConditions.visibilityOfElementLocated(emailInputLoc));
        WebElement passwordInput = driver.findElement(passwordInputLoc);

        emailInput.sendKeys(email);
        passwordInput.sendKeys(password);

        WebElement loginButton = wait.until(ExpectedConditions.elementToBeClickable(loginButtonLoc));

        // Actions ile güvenli tıklama
        new Actions(driver)
                .moveToElement(loginButton)
                .click()
                .perform();
    }
}