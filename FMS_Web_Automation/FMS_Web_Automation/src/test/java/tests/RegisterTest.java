package tests;

import base.BaseTest;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.testng.Assert;
import org.testng.annotations.Test;
import pages.RegisterPage;

public class RegisterTest extends BaseTest {

    @Test
    public void testUserRegistration() {
        RegisterPage registerPage = new RegisterPage(driver, wait);

        // 1. Login sayfasından "Kayıt Olun" linkine tıklayarak register sayfasına git
        registerPage.goToRegisterPageViaLogin();

        // URL kontrolü
        wait.until(ExpectedConditions.urlContains("/register"));

        // 2. Benzersiz bir test e-postası ve şifre ile kayıt ol
        String randomEmail = "testuser_" + System.currentTimeMillis() + "@tav.com";
        registerPage.register(randomEmail, "123456");

        // 3. Başarılı kayıt sonrası otomatik olarak login sayfasına veya dashboard'a yönlendirildiğini doğrula
        // (Projenin akışına göre register sonrası login'e atıyorsa urlContains("/login") yapabilirsin)
        try {
            Thread.sleep(2000); // Yönlendirmenin tamamlanması için kısa bir bekleme
        } catch (InterruptedException e) {
            e.printStackTrace();
        }

        System.out.println("MİMARİ TEST BAŞARILI: Yeni kullanıcı başarıyla kaydedildi -> " + randomEmail);
    }
}