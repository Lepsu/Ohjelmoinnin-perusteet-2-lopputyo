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
    string filename = "HenkiloTiedot.json"; // tiedoston nimi johonka salattut henkil� tiedot menev�t
    string loki = "lokiTiedot.txt"; // tiedoston nimi minne loki tiedot tallnnetaan
    private MainPage _mainPage; // luodaan mainbage muuttuja jotta voidaan liikutella tietoa mainbagelta contentbagelle

    // Salaukseen k�ytett�v�t avaimet
    byte[] key = Encoding.UTF8.GetBytes("yourSecretKey123"); // salauksen tekoon tarvittavasalaus avain
    byte[] iv = Encoding.UTF8.GetBytes("yourIV456"); // salauksen purkuun tarvittava salausavain
    

    public List<HenkiloTiedot> HenkiloLista { get; set; } // luodaan henkil�tiedot muuttujalle Lista objekti

    // t�m� k�ynnist�� Kirjautunutsis��n sivun (contentpage). Ajetaan aina kun sivu k�ynnistet��n (kirjaudutaan sis��n) 
    public KirjautunutSisaan(MainPage mainPage)
	{
		InitializeComponent();
        

        _mainPage = mainPage;
        // annetaan toiminto muuttujalle tapahtuma teksti joka v�litet��n LokiTiedot funktiolle joka kirjaa tapahtuman lokitietoihin.
        // t�ss� kohti lokiin tulee tieto kuka kirjautui sis��n.
        string toiminto = "Kirjautui Sis��n";
        LokiTedot(toiminto);
    }
    // kun hae nappia painetaan t�m�n lohkon sis�ll� tapahtuvat komennot toteutuvat.
    private void Hae_Clicked(object sender, EventArgs e)
    {
        
        string fullpath = Path.Combine(path, filename); // polku mist� tallennetut henkil� tiedot haetaan listview komponenttiin
        if (File.Exists(fullpath)) // ehto lause joka tarkistaa onko kyseist� tiedostoa olemassa. jos on toteutetaan if lauseen lohko.
        {
            // toiminto muuttuja joka v�litet��n lokitiedot funktiolle. ett� n�hd��n mit� tehtiin.
            string toiminto = "Haki henkil�st� tiedot";
            string jsonText = File.ReadAllText(fullpath); // Luetaan json tiedostossa oleva teksti sille annetusta polusta
            jsonText = DecryptText(jsonText, key, iv); // vied��n haettu teksti kryptauksen aukaisu funktiolle ja k��nnet��n nirmaaliin luettavaan muotoon
            HenkiloLista = JsonSerializer.Deserialize<List<HenkiloTiedot>>(jsonText); // v�litet��n unkryptattu teksti lista muuttujalle
            MyListView.ItemsSource = HenkiloLista; // vied��n henkil�st� lista listview komponenttiin k�ytett�v�ksi ja n�ht�v�ksi
            LokiTedot(toiminto); // kutsutaan lokitieto funktiota ja v�litet��n sille toiminto tieto.
        }
    }
    
    // kun muokkaa nappia painetaan suoritetaan seuraavan lohkon sis�lt�
    private async void Muokkaa_Clicked(object sender, EventArgs e)
    {
        string fullpath = Path.Combine(path, filename); // polku mist� tallennetut henkil� tiedot haetaan
        if (MyListView.SelectedItem is HenkiloTiedot selectedPerson) // if lauseen toteutetaan jos listview komponentista on valittu muokattava kohde 
        {
            // seuraavissa k�skyiss� siirret��n entryiss� oleva teksti selectedPerson muuttujaan niille kuuluville paikoille.
            selectedPerson.etunimet = EtunimetEntry.Text;
            selectedPerson.sukunimi = SukunimiEntry.Text;
            selectedPerson.kutsumanimi = KutsumanimiEntry.Text;
            try
            {
                HetuTarkistus(); // Hetun tarkastus funkktio miss� tarkistetaan onko annettu hetu oikein.
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
                selectedPerson.postinumero = int.Parse(PostinumeroEntry.Text); // parsitaan entryn string muotoinen teksti int muotoon ja samalla tarkistetaan ett� olihan tekstiss� vain numeroita
            }
            catch
            {
                await DisplayAlert("VIRHE!", "Postinumero virheellinen", "OK"); // virhe ilmoitus pop up joka ilmoittaa k�ytt�j�lle jos postinumerossa ei ollut vain numeroita
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

            // P�ivit� n�ytett�v� lista
            MyListView.ItemsSource = null;
            MyListView.ItemsSource = HenkiloLista;

            // Tallennetaan muokatut tiedot takaisin json tiedostoon
            string json = JsonSerializer.Serialize(HenkiloLista, new JsonSerializerOptions { WriteIndented = true });
            json = EncryptText(json, key, iv); // k�ytet��n teksti kryptaus funktiossa ennen tiedsotoon kirjoitusta
            File.WriteAllText(fullpath, json); // kirjoitetaan kryptattu teksti json tiedostoon
            // Muuttuja joka v�litet��n lokitiedot muuttujalle mit� tehtiin
            string toiminto = "Muokkasi Henkil�n " + selectedPerson.etunimet + " " + selectedPerson.sukunimi;
            LokiTedot(toiminto); // kutsutaan lokitiedot funktiota ja v�litet��n sille toiminto muuttja
            TyhjennaKentat(); // kutsutaan Tyhjenn�Kent�t funktiota joka tyhjent�� kaikki Entry kent�t sovelluksesta.
        }
        else
        {
            await DisplayAlert("Varoitus", "Valitse henkil� muokataksesi.", "OK"); // pop up ilmoitus joka tulee jos listview listasta ei oltu valittu muokattavaa henkil��
        }
    }
    // seuraava lohko suoritetaan kun tallenna painiketta painetaan
    private void Tallenna_Clicked(object sender, EventArgs e)
    {
        TiedostoonKirjoitus(); // kutsutaan tiedostoon tallennus funktiota joka tallentaa tiedot.
        
    }

    // Kirjaa k�ytt�j�n ulos sovelluksesta ja siirtyy takasin kirjaudu sis��n sivulle Kirhjaudu ulos nappia painettaessa
    private async void KirjauduUlos_Clicked(object sender, EventArgs e)
    {
        string toiminto = "Kirjautui Ulos "; // toiminto muuttuja jolle annteaan t�m�n lohkon toiminto teksti
        LokiTedot(toiminto); // Kutsutaan Lokitiedot Funktiota ja v�litet��n sille toimnto muuttujan teksti
        await Navigation.PopAsync(); // Siirt�� k�ytt�j�n takasin kirjaudu sis��n sivulle.
    }
    // Kirjaa k�ytt�j�n ulos ja sulkee sovelluksen Lopeta nappia painettaessa kirjautunut sis��n sivulla
    private void LopetakirjautunutSis��n_Clicked(object sender, EventArgs e)
    {
        string toiminto = "Kirjautui ulos ja sulki sovelluksen"; // Toiminto muuttujalle annetaan sille kuuluva toiminto teksti.
        LokiTedot(toiminto); // kutsutaan Lokitiedot funktiota ja v�litet��n sille toimnto muuttuja
        App.Current.Quit(); // sulkee sovelluksen kokonaan
    }
    // tiedostoon kirjoitus funktio
    public async void TiedostoonKirjoitus()
    {
        
        string fullpath = Path.Combine(path, filename); // Tiedoston sijainti polku muuttuja
        string json; // luodaan json niminen string muuttuja
        List<HenkiloTiedot> people; // Luodaan Henkil�tiedot luokalle Lista nimelt� people


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
        // luodaan uusi henkil�tiedot tietue nimelt� henkil�
        HenkiloTiedot henkilo = new HenkiloTiedot();
        // seuraavat henkilo. kohdat t�ytt�v�t tietueeseen entry kentist� niille kuuluvaa tietoa
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
            henkilo.postinumero = int.Parse(PostinumeroEntry.Text); // muutetaan string muotoinen teksti int muotoon ja ja samalla tarkastetaan try lohkon sis�ll� oliko kent�ss� vain numeroita.
        }
        catch
        {
            await DisplayAlert("VIRHE!", "Postinumero virheellinen", "OK"); // virhe ilmoitus pop up jos postinumerossa oli muita merkkej� kuin numeroita.
            return; // palauttaa virheen ja keskeytt�� suorituksen
        }
        henkilo.postitoimipaikka = PostitoimipaikkaEntry.Text;
        henkilo.alkamispaiva = AlkamispaivaEntry.Text;
        henkilo.paattymispaiva = PaattymispaivaEntry.Text;
        henkilo.nimike = NimikeEntry.Text;
        henkilo.yksikko = YksikkoEntry.Text;
        try // tarkistus lohko jossa kutsutaan Sy�tteen tarkastus funktiota joka taksitaa onhan kaikissa halutuissa kentiss� teksti� jos ei niin k�ytt�j�lle tulee ilmoitus puuttuvasta tiedota
        {
            Syotteentarkistus();
        }
        catch (Exception ex)
        {
            await DisplayAlert("VIRHE!", ex.Message, "OK"); // virheen ilmoitus pop up ikkuna komento jos  tarkastuksessa ilmeni virhe.
            return;
        }
        people.Add(henkilo); // lis�� henkil�n tiedot henkil�tieto listaan
        string toiminto = "Tallensi Henkil�n " + henkilo.etunimet + " " + henkilo.sukunimi; // toiminto muuttuja jolle annetaan teksti mit� tehtiin

        // Tallenna p�ivitetty tieto takaisin JSON-tiedostoon
        json = JsonSerializer.Serialize(people, new JsonSerializerOptions { WriteIndented = true }); // serialisoidaan people lista json muotoon
        // "kryptataan" tiedot ennen kirjoitusta tiedostoon
        json = EncryptText(json, key, iv); // vied��n teksti kryptaus funktioon joka kryptaa sen
        File.WriteAllText(fullpath, json); // kirjoitetaan kryptatty lista json tiedostoon ja tallennetaan sille annettuun polkuun
        LokiTedot(toiminto); // lokitiedot funktio jolle v�litet��n toimnto muuttuja
        TyhjennaKentat(); // kutsutaan funktiota joka tyhjent�� entry kent�t
        
    }
    // T�ss� alkamisp�iv�pickerist� valittu p�iv�m��r� muutetaan string muotoon ja kirjoitetaan alkamisp�iv� entryyn
    private void AlkamispaivaPicker_DateSelected(object sender, DateChangedEventArgs e)
    {
        AlkamispaivaEntry.Text = e.NewDate.ToString("d");
    }

    // t�ss� loppumisp�iv�pickerist� valittu p�iv� muutetaan string muotoon ja kirjoitetaan p��ttymisp�iv� entryyn
    private void PaattymispaivaPicker_DateSelected(object sender, DateChangedEventArgs e)
    {
        PaattymispaivaEntry.Text = e.NewDate.ToString("d");
    }
    // t�ss� nimikepickerist� valittu nimike muutetaan string muotoon ja kirjoitetaan nimike entryyn
    private void NimikePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        NimikeEntry.Text = NimikePicker.SelectedItem?.ToString();
    }
    // t�ss� yksikk�pickerist� valittu yksikk� muutetaan string muotoon ja kirjoitetaan yksikk� entryyn
    private void YksikkoPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        YksikkoEntry.Text = YksikkoPicker.SelectedItem?.ToString();
    }
    // tyhjenn� kent�t funktio joka kirjoittaa jokaiseen entryyn null tietoa kun sit� kutsutaan
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
        int jj; // jako j��nn�s
        int luku; // numero sarjksi muutettu hetu ilman tarkastus ja v�limerkkej�
        char t = ' '; // tarkastus merkki
        string ppkkvvnnn = ""; // p�iv�m��r� kuukausi vuosi ja j�rjestys numero hetusta


        string annettuHetu = HenkilotunnusEntry.Text;  // siirret��n entryst� hetu annettu hetu muuttujaan
        if ( annettuHetu == null) // tarkistetaan oliko annettu merkkej� hetu osioon
        {
            throw new Exception("Anna Hetu"); // heitet��n virhe ilmoitus jos if lause toteutuu
        }
        
            for (int i = 0; annettuHetu.Length > i; i++) // py�ritet��n for silmukassa hetu l�pi ja tarkastetaan mik� tarkastus merkki oli
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

            luku = int.Parse(ppkkvvnnn); // siirret��n numerot hetusta luku muuttjaan
            jj = luku % 31; // jaetaan hetun luku 31 ja siirret��n jakoj��nn�s jj muuttjaan

            if (t != tarkastus[jj]) // jos t muuttja ja jj muuttuja eri arvoisia toteuttetaan if lohko
            {
                throw new Exception("hetu oli v��rin"); // heitet��n pop up ilmoitus ett� hetu oli v��rin
            }

            return; //palautetaan tarkastuksen tiedot
        
        
    }
        
     // kryptaus funktio 
    static string EncryptText(string input, byte[] key, byte[] iv)
    {
        char[] chars = input.ToCharArray(); // muutetaan l�hetetty teksti char taulukkoon

        for (int i = 0; i < chars.Length; i++) // py�ritet��n char taulukko kokonaan l�pi ja ja lis�t��n jokaisen taulukon merkin arvoa kahdella
        {
            // lis�� merkin arvoa kahdella kullekin merkille
            chars[i] = (char)(chars[i] + 2);
        }

        return new string(chars); // palautetaan muutettu taulukko
    }
    // kryptauksen purku funktio sama toimnta kuin kryptauksessa mutta v�hennet��n jokaisen merkin arvoa 2 jolloin saadaan alkupeir�inen normaali teksti k�ytt��n
    static string DecryptText(string input, byte[] key, byte[] iv)
    {
        char[] chars = input.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            // Palauta alkuper�inen merkki
            chars[i] = (char)(chars[i] - 2);
        }

        return new string(chars);
    }
    // Listviewin kohteen klikkaus funktio. toteutetaan kun klikataan jotain henkill�� list viewist�
    private void MyListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is HenkiloTiedot selectedPerson)
        {
            // T�ytet��n Entry kent�t henkil�n tiedoilla jotta niit� voidaan muokata tai poistaa.
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
        string fullpath = Path.Combine(path, filename); // alustetaan polku muuttujaan sille kuuluva polku mist� tietoja haetaan
        // Tarkista, onko jotain valittuna ListView:ss�
        if (MyListView.SelectedItem is HenkiloTiedot selectedPerson) // toteutetaan jos jotain oli valittuna listview komponentissa
        {
            bool userResponse = await DisplayAlert("Varmistus", "Haluatko varmasti poistaa henkil�n?", "Kyll�", "Ei"); // boolean muuttuja joka l�hett�� pop up kyselyn oletko varma ett� haluat poistaa henkil�n tiedot kokonaan

            if (userResponse) // if lause toteutetaan jos hnekil� vastasi kyll� poistamis kysymykseen
            {
                // Poistaa henkil�n valitusta tiedostosta
                HenkiloLista.Remove(selectedPerson);

                // P�ivit� ListView, jotta poistettu henkil� ei n�y
                MyListView.ItemsSource = null;
                MyListView.ItemsSource = HenkiloLista;

                // Tallentaa p�ivitetyn listan takaisin JSON-tiedostoon
                string json = JsonSerializer.Serialize(HenkiloLista, new JsonSerializerOptions { WriteIndented = true });
                json = EncryptText(json, key, iv); // kutsutaan kryptaus funktiota ja v�litet��n teksti
                File.WriteAllText(fullpath, json); // kirjoitetaan kryptattu teksti json tiedostoon
                string toiminto = "Posti Henkil�n " + selectedPerson.etunimet + " " + selectedPerson.sukunimi; //toiminto muuttujalle annetaan sille kuuluva teksti
                LokiTedot(toiminto); // kutsutaan lokiTiedot funktiota ja v�litet��n toimto tieto
                TyhjennaKentat(); // kutsutaan tyhjenn�kent�t funktiota
            }

            // tyhjennet�n listview valinta
            MyListView.SelectedItem = null;
        }
        else
        {
            await DisplayAlert("Varoitus", "Valitse henkil� poistaaksesi.", "OK"); // pop up Joka ilmoittaa jos ei oltu valittu poistettavaa henkil�� list viewist�.
        }
    }

    // napin toimnta funktio joka j�rjest�� henkil�t nimen mukaan laksevaan j�rjestykseen
    private void KnimiLaskeva_Clicked(object sender, EventArgs e)
    {
        var sortedByKutsumanimiDescending = HenkiloLista.OrderByDescending(person => person.kutsumanimi).ToList();
        MyListView.ItemsSource = sortedByKutsumanimiDescending;

    }
    // napin toimnta funktio joka j�rjest�� henkil�t nimen mukaan nousevaan j�rjestykseen
    private void KnimiNouseva_Clicked(object sender, EventArgs e)
    {
        var sortedByKutsumanimiAscending = HenkiloLista.OrderBy(person => person.kutsumanimi).ToList();
        MyListView.ItemsSource = sortedByKutsumanimiAscending;
    }
    // napin toimnta funktio joka j�rjest�� henkil�t sukunimen mukaan laksevaan j�rjestykseen
    private void SnimiLaskeva_Clicked(object sender, EventArgs e)
    {
        var sortedBySukunimiDescending = HenkiloLista.OrderByDescending(person => person.sukunimi).ToList();
        MyListView.ItemsSource = sortedBySukunimiDescending;
    }
    // napin toimnta funktio joka j�rjest�� henkil�t sukunimen mukaan nousevaan j�rjestykseen
    private void SnimiNouseva_Clicked(object sender, EventArgs e)
    {
        var sortedBySukunimiAscending = HenkiloLista.OrderBy(person => person.sukunimi).ToList();
        MyListView.ItemsSource = sortedBySukunimiAscending;
    }
    // napin toimnta funktio joka j�rjest�� henkil�t nimikkeen mukaan laksevaan j�rjestykseen
    private void nimikeLaskeva_Clicked(object sender, EventArgs e)
    {
        var sortedByNimikeDescending = HenkiloLista.OrderByDescending(person => person.nimike).ToList();
        MyListView.ItemsSource = sortedByNimikeDescending;
    }
    // napin toimnta funktio joka j�rjest�� henkil�t nimikkeen mukaan nousevaan j�rjestykseen
    private void nimikeNouseva_Clicked(object sender, EventArgs e)
    {
        var sortedByNimikeAscending = HenkiloLista.OrderBy(person => person.nimike).ToList();
        MyListView.ItemsSource = sortedByNimikeAscending;
    }
    // funktio jota kutsutaan kun kirjoitetaan teksti� postinumero kentt��n
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
            // Tarkastetaan, onko kirjoitettu v�hint��n kaksi merkki� ja ettei NewTextValue ole null
            if (!string.IsNullOrEmpty(e.NewTextValue) && e.NewTextValue.Length >= 2)
            {
                // Otetaan kaksi ensimm�ist� merkki� postinumerosta
                string kaksiEnsimm�ist�Numerota = e.NewTextValue.Substring(0, 2);
                if (HenkiloLista != null)
                {
                    // Tarkastetaan, onko postinumero tallennettu JSON-tiedostoon
                    HenkiloTiedot henkilo = HenkiloLista.FirstOrDefault(p => p.postinumero.ToString().StartsWith(kaksiEnsimm�ist�Numerota));
                    if (henkilo != null) // suoritetaan if lause jos postinumero l�ytyi listasta
                    {
                        PostitoimipaikkaEntry.Text = henkilo.postitoimipaikka; // kirjoittaa postitoimipaikka kentt��n ehdotetun kaupungin
                    }
                }    
            }
            else
            {
                // Tyhjenn� postitoimipaikka-kentt�, kun kirjoitetaan alle kaksi merkki� tai NewTextValue on null
                PostitoimipaikkaEntry.Text = string.Empty;
            }
        }
        catch (Exception ex)
        {
            
            
            DisplayAlert("Virhe", ex.Message, "OK"); // virhe ilmoitus jos postinumeron sy�t�ss� tapahtui virhe
        }
    }
    // lokitiodet funktio joka kirjaa k�ytt�j�n toimntaa kun sit� kutsutaan
    public void LokiTedot(string toiminto)
    {
        string fullpath = Path.Combine(path, loki); // annetaan muuttujalle polku jonne lokitiedot tallennetaan
        using (StreamWriter sw = new StreamWriter(fullpath, true)) // lis�t��n teksti� tiedostoon
        {
            
            string rivi = ""; // alustetaan rivi muuttuja johon likitioedot kirjoitetaan
            KayttajaTunnus kayttajaa = _mainPage.DataFromMainpage(); // haetaan kirjautuneen k�ytt�j�n k�ytt�j� tunnus mainbagelta
            
                // kirjoitetaan rivi muuttjaan kellon aika p�iv�, kuka oli kirjautuneen ja mit� teki
                rivi = DateTime.Now + " K�ytt�j� " + kayttajaa.Kayttaja.ToString() + " " + toiminto;
                //kirjoitetaan tiedostoon
                sw.WriteLine(rivi);
            
        }
    }
    // sy�tteen tarkastus funktio jota kutsuttaessa se tarksitaa onko kaikissa pakollisissa kentiss� teksti�
    public  void Syotteentarkistus()
    {
        // Jokainen alla oleva if lause tarkistaa onko annetussa kent�ss� teksti� ja jos ei niin palauttaa k�ytt�j�lle virhe ilmoituksen mist� kent�st� teksti puuttui
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
            throw new Exception("Anna Alkamisp�iv�");
        }
        if (NimikeEntry.Text == null || NimikeEntry.Text == "")
        {
            throw new Exception("Anna Nimike");
        }
        if (YksikkoEntry.Text == null || YksikkoEntry.Text == "")
        {
            throw new Exception("Anna Yksikk�");
        }
        return;

    }

}