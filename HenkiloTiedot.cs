using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace final_work
{
    // henkilötieto luokka
    public class HenkiloTiedot
    {
        // Luodaan Henkilötieto luokkaan sille kuuluvat muuttujat
        public string etunimet { get; set; }
        public string sukunimi { get; set; }
        public string kutsumanimi { get; set; }
        public string henkilotunnus { get; set; }
        public string katuosoite { get; set; }
        public int postinumero { get; set; }
        public string postitoimipaikka { get; set; }
        public string alkamispaiva { get; set; }
        public string paattymispaiva { get; set; }
        public string nimike { get; set; }
        public string yksikko { get; set; }
    }
}
