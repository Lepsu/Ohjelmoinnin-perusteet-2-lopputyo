namespace final_work;

public partial class MainPage : ContentPage
{
	// Alustetaan muuttujat
	string kayttaja;
	string salasana;
	public MainPage() // tämä lohko käynnistää kirjaudutaan sisään sivun
	{
		InitializeComponent();
	}


    
    // funktio jota kutsutaan kun kiurjaudutaan sisään nappia painetaan
    private async void KirjauduSisaanNappi_Clicked(object sender, EventArgs e)
    {
        // kirjoitetaan käyttäjän tunnus ja salasana Entryistä aikaisemmin luotuihin muuttujiin
        kayttaja = KayttajaTunnusEntry.Text;
        salasana = SalasanaEntry.Text;
        // Tarkastetaan että kenttiin on syötetty jotain merkkejä, jos ei niin käyttäjälle tulee popUp ilmoitus
        if (salasana == null || kayttaja == null || salasana == "" || kayttaja == "")
        {
            await DisplayAlert("VIRHE!", "Anna Käyttäjätunnus ja salasana", "OK");
        }
        // Tarkastetaan onko käyttäjä tunnus ja salasana oikein (täytyy olla sama teksti kummassakin kentässä)
        // Jos ei, siirrytään else haaraan ja Käyttäjälle tulee PopUp ilmoitus virheestä
        else if (salasana == kayttaja)
        {
            // Tyhjennetään Käyttäjä tunnus ja salasana Entryt
            KayttajaTunnusEntry.Text = null;
            SalasanaEntry.Text = null;

            // Luodaan uusi KirjautunutSisään sivu
            KirjautunutSisaan kirjautunutSisaan = new KirjautunutSisaan(this);

            // Siirrytään uudelle KirjaudutaanSisään sivulle
            await Navigation.PushAsync(kirjautunutSisaan);
        }
        else
        {
            await DisplayAlert("VIRHE!", "Käyttäjätunnus tai Salasana Virheellinen", "OK"); // virhe ilmoitus pop up jos salasana tai käyttäjä tunnus olivat virheelliset
        }
    }
    // käyttäjä tunnus muuttujan luonti jotta saadaan tieto contentpagelle
    private KayttajaTunnus KirjautunutKayttaja;
    // funktio jolla siirretään tietoa Contetbagelle kuka oli kirjautuneena sisään
    public KayttajaTunnus DataFromMainpage()
    {
        KayttajaTunnus kayttajaTunnus = new KayttajaTunnus(); // luodaan uusi käyttäjä tunnus muuttuja
        kayttajaTunnus.Kayttaja = kayttaja; // annetaan käyttäjätunnus tuetueelle käyttäjä tiedot
        return kayttajaTunnus; // palautetaan käyttäjä joka on kirjautuneena sisään
    }

    // kun tarkastus nappia pidetään painettuna salasanakentässä oleva teksti tulee näkyviin
    private void TarkastusNappi_Pressed(object sender, EventArgs e)
    {
        SalasanaEntry.IsPassword = false;
    }
    // kun tarkastus napin painaminen lopetetaan salasana kentän teksti muuttuu takaisin palloiksi
    private void TarkastusNappi_Released(object sender, EventArgs e)
    {
        SalasanaEntry.IsPassword = true;
    }
    // Sammuttaa sovelluksen Kirjautumis sivulta
    private void LopetaKirjautumisSivu_Clicked(object sender, EventArgs e)
    {
        App.Current.Quit();
    }
}

