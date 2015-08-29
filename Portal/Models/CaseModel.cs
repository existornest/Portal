﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Portal.Models
{
    public class CaseModel
    {
        [Key]
        public int ID { get; set; }

        public Guid Guid { get; set; }

        [Display(Name = "Numer sprawy")]
        public string NumerSprawy { get; set; }

        [Display(Name = "Data wpływu")]
        public string DataWplywu { get; set; }

        [Display(Name = "Kontakt")]
        public string Kontakt { get; set; }

        [Display(Name = "Etap sprawy")]
        public int EtapSprawy { get; set; }

        [Display(Name = "Data wysłania umowy")]
        public string DatawyslaniaUmowy { get; set; }

        [Display(Name = "Klient")]
        public string Klient { get; set; }

        [Display(Name = "Właściciel")]
        public string Wlasciciel { get; set; }
    }
}