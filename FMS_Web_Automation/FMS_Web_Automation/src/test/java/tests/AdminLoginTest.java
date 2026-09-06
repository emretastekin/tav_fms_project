package tests;

import base.BaseTest;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.testng.Assert;
import org.testng.annotations.Test;
import pages.DashboardPage;
import pages.LoginPage;

public class AdminLoginTest extends BaseTest {

    @Test
    public void testAdminLogin() throws InterruptedException {
        LoginPage loginPage = new LoginPage(driver, wait);
        DashboardPage dashboardPage = new DashboardPage(driver, wait);

        // 1. Login sayfasına git ve giriş yap
        loginPage.goToLoginPage();
        loginPage.login("test@tav.com", "123456");

        // 2. URL kontrolü ve Dashboard doğrulaması
        wait.until(ExpectedConditions.not(ExpectedConditions.urlContains("/login")));

        Assert.assertTrue(dashboardPage.isAddFlightDisplayed(), "Uçuş Ekle butonu görünmedi!");

        // 3. Güvenli çıkış yap
        dashboardPage.logout();
        wait.until(ExpectedConditions.urlContains("/login"));

        System.out.println("MİMARİ TEST BAŞARILI: Page Object Model ile giriş ve çıkış test edildi!");
    }
}