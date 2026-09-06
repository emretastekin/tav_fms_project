package pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

public class DashboardPage {
    WebDriver driver;
    WebDriverWait wait;

    private By addFlightLinkLoc = By.xpath("//a[contains(text(),'+ Uçuş Ekle')]");
    private By logoutButtonLoc = By.xpath("//button[contains(text(),'Çıkış Yap')]");

    public DashboardPage(WebDriver driver, WebDriverWait wait) {
        this.driver = driver;
        this.wait = wait;
    }

    public boolean isAddFlightDisplayed() {
        WebElement addFlightLink = wait.until(ExpectedConditions.visibilityOfElementLocated(addFlightLinkLoc));
        return addFlightLink.isDisplayed();
    }

    public void logout() {
        WebElement logoutButton = wait.until(ExpectedConditions.elementToBeClickable(logoutButtonLoc));
        logoutButton.click();
    }
}