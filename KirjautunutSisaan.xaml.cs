using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Platform;
using Microsoft.Web.WebView2.Core;
using System;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Windows.Security.Cryptography.Core;

namespace final_work;

public partial class KirjautunutSisaan : ContentPage
{
    // alustetaan muuttujat tiedostoon tallennusta varten
    string path = FileSystem.Current.CacheDirectory; // cache kansion polku minne tiedot tallennetaan
    string filename = "HenkiloTiedot.json"; // tiedoston nimi johonka salattut henkilö tiedot menevät
    string loki = "lokiTiedot.txt"; // tiedoston nimi minne loki tiedot tallnnetaan
    private MainPage _mainPage; // luodaan mainbage muuttuja jotta voidaan liikutella tietoa mainbagelta contentbagelle

    // Salaukseen käytettävät avaimet
    byte[] key = Encoding.UTF8.GetBytes("yourSecretKey123"); // salauksen tekoon tarvittavasalaus avain
    byte[] iv = Encoding.UTF8.GetBytes("yourIV456"); // salauksen purkuun tarvittava salausavain
    

    public List<HenkiloTiedot> HenkiloLista { get; set; } // luodaan henkilötiedot muuttujalle Lista objekti

    // tämä käynnistää Kirjautunutsisään sivun (contentpage). Ajetaan aina kun sivu käynnistetään (kirjaudutaan sisään) 
    public KirjautunutSisaan(MainPage mainPage)
	{
		InitializeComponent();
        

        _mainPage = mainPage;
        // annetaan toiminto muuttujalle tapahtuma teksti joka välitetään LokiTiedot funktiolle joka kirjaa tapahtuman lokitietoihin.
        // tässä kohti lokiin tulee tieto kuka kirjautui sisään.
        string toiminto = "Kirjautui Sisään";
        LokiTedot(toiminto);
    }
    // kun hae nappia painetaan tämän lohkon sisällä tapahtuvat komennot toteutuvat.
    private void Hae_Clicked(object sender, EventArgs e)
    {
        
        string fullpath = Path.Combine(path, filename); // polku mistä tallennetut henkilö tiedot haetaan listview komponenttiin
        if (File.Exists(fullpath)) // ehto lause joka tarkistaa onko kyseistä tiedostoa olemassa. jos on toteutetaan if lauseen lohko.
        {
            // toiminto muuttuja joka välitetään lokitiedot funktiolle. että nähdään mitä tehtiin.
            string toiminto = "Haki henkilöstö tiedot";
            string jsonText = File.ReadAllText(fullpath); // Luetaan json tiedostossa oleva teksti sille annetusta polusta
            jsonText = DecryptText(jsonText, key, iv); // viedään haettu teksti kryptauksen aukaisu funktiolle ja käännetään nirmaaliin luettavaan muotoon
            HenkiloLista = JsonSerializer.Deserialize<List<HenkiloTiedot>>(jsonText); // välitetään unkryptattu teksti lista muuttujalle
            MyListView.ItemsSource = HenkiloLista; // viedään henkilöstö lista listview komponenttiin käytettäväksi ja nähtäväksi
            LokiTedot(toiminto); // kutsutaan lokitieto funktiota ja välitetään sille toiminto tieto.
        }
    }
    
    // kun muokkaa nappia painetaan suoritetaan seuraavan lohkon sisältö
    private async void Muokkaa_Clicked(object sender, EventArgs e)
    {
        string fullpath = Path.Combine(path, filename); // polku mistä tallennetut henkilö tiedot haetaan
        if (MyListView.SelectedItem is HenkiloTiedot selectedPerson) // if lauseen toteutetaan jos listview komponentista on valittu muokattava kohde 
        {
            // seuraavissa käskyissä siirretään entryissä oleva teksti selectedPerson muuttujaan niille kuuluville paikoille.
            selectedPerson.etunimet = EtunimetEntry.Text;
            selectedPerson.sukunimi = SukunimiEntry.Text;
            selectedPerson.kutsumanimi = KutsumanimiEntry.Text;
            try
            {
                HetuTarkistus(); // Hetun tarkastus funkktio missä tarkistetaan onko annettu hetu oikein.
            }
            catch (Exception ex)
            {
                await DisplayAlert("VIRHE!", ex.Message, "OK"); // virheen ilmoitus pop up ikkuna komento jos hetun tarkastuksessa ilmeni virhe.
                return; // palauttaa virhe ilmoituksen ja lopettaan kohdan suorittamisen
            }
            selectedPerson.henkilotunnus = HenkilotunnusEntry.Text;
            selectedPerson.katuosoite = KatuosoiteEntry.Text;
            try
            {
                selectedPerson.postinumero = int.Parse(PostinumeroEntry.Text); // parsitaan entryn string muotoinen teksti int muotoon ja samalla tarkistetaan että olihan tekstissä vain numeroita
            }
            catch
            {
                await DisplayAlert("VIRHE!", "Postinumero virheellinen", "OK"); // virhe ilmoitus pop up joka ilmoittaa käyttäjälle jos postinumerossa ei ollut vain numeroita
                return; // palauttaa virheen ja lopettaan toiminnon suorituksen
            }
            selectedPerson.postitoimipaikka = PostitoimipaikkaEntry.Text;
            selectedPerson.alkamispaiva = AlkamispaivaEntry.Text;
            selectedPerson.paattymispaiva = PaattymispaivaEntry.Text;
            selectedPerson.nimike = NimikeEntry.Text;
            selectedPerson.yksikko = YksikkoEntry.Text;
            try 
            {
                Syotteentarkistus();
            }
            catch (Exception ex)
            {
                await DisplayAlert("VIRHE!", ex.Message, "OK"); // virheen ilmoitus pop up ikkuna komento jos  tarkastuksessa ilmeni virhe.
                return;
            }

            // Päivitä näytettävä lista
            MyListView.ItemsSource = null;
            MyListView.ItemsSource = HenkiloLista;

            // Tallennetaan muokatut tiedot takaisin json tiedostoon
            string json = JsonSerializer.Serialize(HenkiloLista, new JsonSerializerOptions { WriteIndented = true });
            json = EncryptText(json, key, iv); // käytetään teksti kryptaus funktiossa ennen tiedsotoon kirjoitusta
            File.WriteAllText(fullpath, json); // kirjoitetaan kryptattu teksti json tiedostoon
            // Muuttuja joka välitetään lokitiedot muuttujalle mitä tehtiin
            string toiminto = "Muokkasi Henkilön " + selectedPerson.etunimet + " " + selectedPerson.sukunimi;
            LokiTedot(toiminto); // kutsutaan lokitiedot funktiota ja välitetään sille toiminto muuttja
            TyhjennaKentat(); // kutsutaan TyhjennäKentät funktiota joka tyhjentää kaikki Entry kentät sovelluksesta.
        }
        else
        {
            await DisplayAlert("Varoitus", "Valitse henkilö muokataksesi.", "OK"); // pop up ilmoitus joka tulee jos listview listasta ei oltu valittu muokattavaa henkilöä
        }
    }
    // seuraava lohko suoritetaan kun tallenna painiketta painetaan
    private void Tallenna_Clicked(object sender, EventArgs e)
    {
        TiedostoonKirjoitus(); // kutsutaan tiedostoon tallennus funktiota joka tallentaa tiedot.
        
    }

    // Kirjaa käyttäjän ulos sovelluksesta ja siirtyy takasin kirjaudu sisään sivulle Kirhjaudu ulos nappia painettaessa
    private async void KirjauduUlos_Clicked(object sender, EventArgs e)
    {
        string toiminto = "Kirjautui Ulos "; // toiminto muuttuja jolle annteaan tämän lohkon toiminto teksti
        LokiTedot(toiminto); // Kutsutaan Lokitiedot Funktiota ja välitetään sille toimnto muuttujan teksti
        await Navigation.PopAsync(); // Siirtää käyttäjän takasin kirjaudu sisään sivulle.
    }
    // Kirjaa käyttäjän ulos ja sulkee sovelluksen Lopeta nappia painettaessa kirjautunut sisään sivulla
    private void LopetakirjautunutSisään_Clicked(object sender, EventArgs e)
    {
        string toiminto = "Kirjautui ulos ja sulki sovelluksen"; // Toiminto muuttujalle annetaan sille kuuluva toiminto teksti.
        LokiTedot(toiminto); // kutsutaan Lokitiedot funktiota ja välitetään sille toimnto muuttuja
        App.Current.Quit(); // sulkee sovelluksen kokonaan
    }
    // tiedostoon kirjoitus funktio
    public async void TiedostoonKirjoitus()
    {
        
        string fullpath = Path.Combine(path, filename); // Tiedoston sijainti polku muuttuja
        string json; // luodaan json niminen string muuttuja
        List<HenkiloTiedot> people; // Luodaan Henkilötiedot luokalle Lista nimeltä people


        if (File.Exists(fullpath)) // Jos Tiedosto on olemassa suoritetaan if lause.
        {
            // Lue olemassa olevat tiedot JSON-tiedostosta
            json = File.ReadAllText(fullpath);
            // Puretaan "kryptaus" ennen deserilisointia
            json = DecryptText(json, key, iv);
            people = JsonSerializer.Deserialize<List<HenkiloTiedot>>(json);
            
        }
        else // suoritetaan else lohko jos if lauseen tiedosto polussa ei ollut tiedostoa
        {
            //luo uusi lista
            people = new List<HenkiloTiedot>();
        }
        // luodaan uusi henkilötiedot tietue nimeltä henkilö
        HenkiloTiedot henkilo = new HenkiloTiedot();
        // seuraavat henkilo. kohdat täyttävät tietueeseen entry kentistä niille kuuluvaa tietoa
        henkilo.etunimet = EtunimetEntry.Text;
        henkilo.sukunimi = SukunimiEntry.Text;
        henkilo.kutsumanimi = KutsumanimiEntry.Text;
        try
        {
            HetuTarkistus(); // Kutsutaan hetun tarkastus funktiota joka tarkastaa onko hetu oikein
        }
        catch (Exception ex)
        {
            await DisplayAlert("VIRHE!", ex.Message, "OK"); // virhe ilmoitus pop up joka kertoo jos hetun tarkastuksessa tuli vika
            return; // palauttaa virhe ilmoituksen ja keskeyttaa suorituksen
        }
        henkilo.henkilotunnus = HenkilotunnusEntry.Text;
        henkilo.katuosoite = KatuosoiteEntry.Text;
        try
        {
            henkilo.postinumero = int.Parse(PostinumeroEntry.Text); // muutetaan string muotoinen teksti int muotoon ja ja samalla tarkastetaan try lohkon sisällä oliko kentässä vain numeroita.
        }
        catch
        {
            await DisplayAlert("VIRHE!", "Postinumero virheellinen", "OK"); // virhe ilmoitus pop up jos postinumerossa oli muita merkkejä kuin numeroita.
            return; // palauttaa virheen ja keskeyttää suorituksen
        }
        henkilo.postitoimipaikka = PostitoimipaikkaEntry.Text;
        henkilo.alkamispaiva = AlkamispaivaEntry.Text;
        henkilo.paattymispaiva = PaattymispaivaEntry.Text;
        henkilo.nimike = NimikeEntry.Text;
        henkilo.yksikko = YksikkoEntry.Text;
        try // tarkistus lohko jossa kutsutaan Syötteen tarkastus funktiota joka taksitaa onhan kaikissa halutuissa kentissä tekstiä jos ei niin käyttäjälle tulee ilmoitus puuttuvasta tiedota
        {
            Syotteentarkistus();
        }
        catch (Exception ex)
        {
            await DisplayAlert("VIRHE!", ex.Message, "OK"); // virheen ilmoitus pop up ikkuna komento jos  tarkastuksessa ilmeni virhe.
            return;
        }
        people.Add(henkilo); // lisää henkilön tiedot henkilötieto listaan
        string toiminto = "Tallensi Henkilön " + henkilo.etunimet + " " + henkilo.sukunimi; // toiminto muuttuja jolle annetaan teksti mitä tehtiin

        // Tallenna päivitetty tieto takaisin JSON-tiedostoon
        json = JsonSerializer.Serialize(people, new JsonSerializerOptions { WriteIndented = true }); // serialisoidaan people lista json muotoon
        // "kryptataan" tiedot ennen kirjoitusta tiedostoon
        json = EncryptText(json, key, iv); // viedään teksti kryptaus funktioon joka kryptaa sen
        File.WriteAllText(fullpath, json); // kirjoitetaan kryptatty lista json tiedostoon ja tallennetaan sille annettuun polkuun
        LokiTedot(toiminto); // lokitiedot funktio jolle välitetään toimnto muuttuja
        TyhjennaKentat(); // kutsutaan funktiota joka tyhjentää entry kentät
        
    }
    // Tässä alkamispäiväpickeristä valittu päivämäärä muutetaan string muotoon ja kirjoitetaan alkamispäivä entryyn
    private void AlkamispaivaPicker_DateSelected(object sender, DateChangedEventArgs e)
    {
        AlkamispaivaEntry.Text = e.NewDate.ToString("d");
    }

    // tässä loppumispäiväpickeristä valittu päivä muutetaan string muotoon ja kirjoitetaan päättymispäivä entryyn
    private void PaattymispaivaPicker_DateSelected(object sender, DateChangedEventArgs e)
    {
        PaattymispaivaEntry.Text = e.NewDate.ToString("d");
    }
    // tässä nimikepickeristä valittu nimike muutetaan string muotoon ja kirjoitetaan nimike entryyn
    private void NimikePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        NimikeEntry.Text = NimikePicker.SelectedItem?.ToString();
    }
    // tässä yksikköpickeristä valittu yksikkö muutetaan string muotoon ja kirjoitetaan yksikkö entryyn
    private void YksikkoPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        YksikkoEntry.Text = YksikkoPicker.SelectedItem?.ToString();
    }
    // tyhjennä kentät funktio joka kirjoittaa jokaiseen entryyn null tietoa kun sitä kutsutaan
    public void TyhjennaKentat()
    {
        EtunimetEntry.Text = null;
        SukunimiEntry.Text = null;
        KutsumanimiEntry.Text = null;
        HenkilotunnusEntry.Text = null;
        KatuosoiteEntry.Text = null;
        PostinumeroEntry.Text = null;
        PostitoimipaikkaEntry.Text = null;
        AlkamispaivaEntry.Text = null;
        PaattymispaivaEntry.Text = null;
        NimikeEntry.Text = null;
        YksikkoEntry.Text = null;
    }
    // hetun tarkastus funktio
    public void HetuTarkistus()
    {
        // Char muotoinen taulukko minne tallennettu tarkistus merkit
        char[] tarkastus = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y' };
        // alustetaan tarvittavat muuttujat
        int jj; // jako jäännös
        int luku; // numero sarjksi muutettu hetu ilman tarkastus ja välimerkkejä
        char t = ' '; // tarkastus merkki
        string ppkkvvnnn = ""; // päivämäärä kuukausi vuosi ja järjestys numero hetusta


        string annettuHetu = HenkilotunnusEntry.Text;  // siirretään entrystä hetu annettu hetu muuttujaan
        if ( annettuHetu == null) // tarkistetaan oliko annettu merkkejä hetu osioon
        {
            throw new Exception("Anna Hetu"); // heitetään virhe ilmoitus jos if lause toteutuu
        }
        
            for (int i = 0; annettuHetu.Length > i; i++) // pyöritetään for silmukassa hetu läpi ja tarkastetaan mikä tarkastus merkki oli
            {
                if (i != 6 && i != 10)
                {
                    ppkkvvnnn = ppkkvvnnn + annettuHetu[i];
                }
                if (i == 10)
                {
                    t = annettuHetu[i];
                }
            }

            luku = int.Parse(ppkkvvnnn); // siirretään numerot hetusta luku muuttjaan
            jj = luku % 31; // jaetaan hetun luku 31 ja siirretään jakojäännös jj muuttjaan

            if (t != tarkastus[jj]) // jos t muuttja ja jj muuttuja eri arvoisia toteuttetaan if lohko
            {
                throw new Exception("hetu oli väärin"); // heitetään pop up ilmoitus että hetu oli väärin
            }

            return; //palautetaan tarkastuksen tiedot
        
        
    }
        
     // kryptaus funktio 
    static string EncryptText(string input, byte[] key, byte[] iv)
    {
        char[] chars = input.ToCharArray(); // muutetaan lähetetty teksti char taulukkoon

        for (int i = 0; i < chars.Length; i++) // pyöritetään char taulukko kokonaan läpi ja ja lisätään jokaisen taulukon merkin arvoa kahdella
        {
            // lisää merkin arvoa kahdella kullekin merkille
            chars[i] = (char)(chars[i] + 2);
        }

        return new string(chars); // palautetaan muutettu taulukko
    }
    // kryptauksen purku funktio sama toimnta kuin kryptauksessa mutta vähennetään jokaisen merkin arvoa 2 jolloin saadaan alkupeiräinen normaali teksti käyttöön
    static string DecryptText(string input, byte[] key, byte[] iv)
    {
        char[] chars = input.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            // Palauta alkuperäinen merkki
            chars[i] = (char)(chars[i] - 2);
        }

        return new string(chars);
    }
    // Listviewin kohteen klikkaus funktio. toteutetaan kun klikataan jotain henkillöä list viewistä
    private void MyListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is HenkiloTiedot selectedPerson)
        {
            // Täytetään Entry kentät henkilön tiedoilla jotta niitä voidaan muokata tai poistaa.
            EtunimetEntry.Text = selectedPerson.etunimet;
            SukunimiEntry.Text = selectedPerson.sukunimi;
            KutsumanimiEntry.Text = selectedPerson.kutsumanimi;
            HenkilotunnusEntry.Text = selectedPerson.henkilotunnus;
            KatuosoiteEntry.Text = selectedPerson.katuosoite;
            PostinumeroEntry.Text = selectedPerson.postinumero.ToString();
            PostitoimipaikkaEntry.Text = selectedPerson.postitoimipaikka;
            AlkamispaivaEntry.Text = selectedPerson.alkamispaiva;
            PaattymispaivaEntry.Text = selectedPerson.paattymispaiva;
            NimikeEntry.Text = selectedPerson.nimike;
            YksikkoEntry.Text = selectedPerson.yksikko;

            
            
        }
    }
    // Poista napin toimnta funktio. suoritetaan kun poista nappia painetaan
    private async void Poista_Clicked(object sender, EventArgs e)
    {
        string fullpath = Path.Combine(path, filename); // alustetaan polku muuttujaan sille kuuluva polku mistä tietoja haetaan
        // Tarkista, onko jotain valittuna ListView:ssä
        if (MyListView.SelectedItem is HenkiloTiedot selectedPerson) // toteutetaan jos jotain oli valittuna listview komponentissa
        {
            bool userResponse = await DisplayAlert("Varmistus", "Haluatko varmasti poistaa henkilön?", "Kyllä", "Ei"); // boolean muuttuja joka lähettää pop up kyselyn oletko varma että haluat poistaa henkilön tiedot kokonaan

            if (userResponse) // if lause toteutetaan jos hnekilö vastasi kyllä poistamis kysymykseen
            {
                // Poistaa henkilön valitusta tiedostosta
                HenkiloLista.Remove(selectedPerson);

                // Päivitä ListView, jotta poistettu henkilö ei näy
                MyListView.ItemsSource = null;
                MyListView.ItemsSource = HenkiloLista;

                // Tallentaa päivitetyn listan takaisin JSON-tiedostoon
                string json = JsonSerializer.Serialize(HenkiloLista, new JsonSerializerOptions { WriteIndented = true });
                json = EncryptText(json, key, iv); // kutsutaan kryptaus funktiota ja välitetään teksti
                File.WriteAllText(fullpath, json); // kirjoitetaan kryptattu teksti json tiedostoon
                string toiminto = "Posti Henkilön " + selectedPerson.etunimet + " " + selectedPerson.sukunimi; //toiminto muuttujalle annetaan sille kuuluva teksti
                LokiTedot(toiminto); // kutsutaan lokiTiedot funktiota ja välitetään toimto tieto
                TyhjennaKentat(); // kutsutaan tyhjennäkentät funktiota
            }

            // tyhjennetän listview valinta
            MyListView.SelectedItem = null;
        }
        else
        {
            await DisplayAlert("Varoitus", "Valitse henkilö poistaaksesi.", "OK"); // pop up Joka ilmoittaa jos ei oltu valittu poistettavaa henkilöä list viewistä.
        }
    }

    // napin toimnta funktio joka järjestää henkilöt nimen mukaan laksevaan järjestykseen
    private void KnimiLaskeva_Clicked(object sender, EventArgs e)
    {
        var sortedByKutsumanimiDescending = HenkiloLista.OrderByDescending(person => person.kutsumanimi).ToList();
        MyListView.ItemsSource = sortedByKutsumanimiDescending;

    }
    // napin toimnta funktio joka järjestää henkilöt nimen mukaan nousevaan järjestykseen
    private void KnimiNouseva_Clicked(object sender, EventArgs e)
    {
        var sortedByKutsumanimiAscending = HenkiloLista.OrderBy(person => person.kutsumanimi).ToList();
        MyListView.ItemsSource = sortedByKutsumanimiAscending;
    }
    // napin toimnta funktio joka järjestää henkilöt sukunimen mukaan laksevaan järjestykseen
    private void SnimiLaskeva_Clicked(object sender, EventArgs e)
    {
        var sortedBySukunimiDescending = HenkiloLista.OrderByDescending(person => person.sukunimi).ToList();
        MyListView.ItemsSource = sortedBySukunimiDescending;
    }
    // napin toimnta funktio joka järjestää henkilöt sukunimen mukaan nousevaan järjestykseen
    private void SnimiNouseva_Clicked(object sender, EventArgs e)
    {
        var sortedBySukunimiAscending = HenkiloLista.OrderBy(person => person.sukunimi).ToList();
        MyListView.ItemsSource = sortedBySukunimiAscending;
    }
    // napin toimnta funktio joka järjestää henkilöt nimikkeen mukaan laksevaan järjestykseen
    private void nimikeLaskeva_Clicked(object sender, EventArgs e)
    {
        var sortedByNimikeDescending = HenkiloLista.OrderByDescending(person => person.nimike).ToList();
        MyListView.ItemsSource = sortedByNimikeDescending;
    }
    // napin toimnta funktio joka järjestää henkilöt nimikkeen mukaan nousevaan järjestykseen
    private void nimikeNouseva_Clicked(object sender, EventArgs e)
    {
        var sortedByNimikeAscending = HenkiloLista.OrderBy(person => person.nimike).ToList();
        MyListView.ItemsSource = sortedByNimikeAscending;
    }
    // funktio jota kutsutaan kun kirjoitetaan tekstiä postinumero kenttään
    private void PostinumeroEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        string fullpath = Path.Combine(path, filename); // Tiedoston sijainti polku muuttuja
        string json; // luodaan json niminen string muuttuja
        


        if (File.Exists(fullpath)) // Jos Tiedosto on olemassa suoritetaan if lause.
        {
            // Lue olemassa olevat tiedot JSON-tiedostosta
            json = File.ReadAllText(fullpath);
            // Puretaan "kryptaus" ennen deserilisointia
            json = DecryptText(json, key, iv); // puretaan kryptaus omassa functiossa
            HenkiloLista = JsonSerializer.Deserialize<List<HenkiloTiedot>>(json); // kirjoitetaan listaan json tiedostossa olleet tiedot

        }
        try
        {
            // Tarkastetaan, onko kirjoitettu vähintään kaksi merkkiä ja ettei NewTextValue ole null
            if (!string.IsNullOrEmpty(e.NewTextValue) && e.NewTextValue.Length >= 2)
            {
                // Otetaan kaksi ensimmäistä merkkiä postinumerosta
                string kaksiEnsimmäistäNumerota = e.NewTextValue.Substring(0, 2);
                if (HenkiloLista != null)
                {
                    // Tarkastetaan, onko postinumero tallennettu JSON-tiedostoon
                    HenkiloTiedot henkilo = HenkiloLista.FirstOrDefault(p => p.postinumero.ToString().StartsWith(kaksiEnsimmäistäNumerota));
                    if (henkilo != null) // suoritetaan if lause jos postinumero löytyi listasta
                    {
                        PostitoimipaikkaEntry.Text = henkilo.postitoimipaikka; // kirjoittaa postitoimipaikka kenttään ehdotetun kaupungin
                    }
                }    
            }
            else
            {
                // Tyhjennä postitoimipaikka-kenttä, kun kirjoitetaan alle kaksi merkkiä tai NewTextValue on null
                PostitoimipaikkaEntry.Text = string.Empty;
            }
        }
        catch (Exception ex)
        {
            
            
            DisplayAlert("Virhe", ex.Message, "OK"); // virhe ilmoitus jos postinumeron syötössä tapahtui virhe
        }
    }
    // lokitiodet funktio joka kirjaa käyttäjän toimntaa kun sitä kutsutaan
    public void LokiTedot(string toiminto)
    {
        string fullpath = Path.Combine(path, loki); // annetaan muuttujalle polku jonne lokitiedot tallennetaan
        using (StreamWriter sw = new StreamWriter(fullpath, true)) // lisätään tekstiä tiedostoon
        {
            
            string rivi = ""; // alustetaan rivi muuttuja johon likitioedot kirjoitetaan
            KayttajaTunnus kayttajaa = _mainPage.DataFromMainpage(); // haetaan kirjautuneen käyttäjän käyttäjä tunnus mainbagelta
            
                // kirjoitetaan rivi muuttjaan kellon aika päivä, kuka oli kirjautuneen ja mitä teki
                rivi = DateTime.Now + " Käyttäjä " + kayttajaa.Kayttaja.ToString() + " " + toiminto;
                //kirjoitetaan tiedostoon
                sw.WriteLine(rivi);
            
        }
    }
    // syötteen tarkastus funktio jota kutsuttaessa se tarksitaa onko kaikissa pakollisissa kentissä tekstiä
    public  void Syotteentarkistus()
    {
        // Jokainen alla oleva if lause tarkistaa onko annetussa kentässä tekstiä ja jos ei niin palauttaa käyttäjälle virhe ilmoituksen mistä kentästä teksti puuttui
        if (EtunimetEntry.Text == null || EtunimetEntry.Text == "") 
        {
            throw new Exception("Anna Etunimet");
        }
        if (SukunimiEntry.Text == null || SukunimiEntry.Text == "")
        {
            throw new Exception("Anna Sukunimi");
        }
        if (KutsumanimiEntry.Text == null || KutsumanimiEntry.Text == "")
        {
            throw new Exception("Anna Kutsumanimi");
        }
        if (KatuosoiteEntry.Text == null || KatuosoiteEntry.Text == "")
        {
            throw new Exception("Anna Katuosoite");
        }
        if (PostitoimipaikkaEntry.Text == null || PostitoimipaikkaEntry.Text == "")
        {
            throw new Exception("Anna Postitoimipaikka");
        }
        if (AlkamispaivaEntry.Text == null || AlkamispaivaEntry.Text == "")
        {
            throw new Exception("Anna Alkamispäivä");
        }
        if (NimikeEntry.Text == null || NimikeEntry.Text == "")
        {
            throw new Exception("Anna Nimike");
        }
        if (YksikkoEntry.Text == null || YksikkoEntry.Text == "")
        {
            throw new Exception("Anna Yksikkö");
        }
        return;

    }

}