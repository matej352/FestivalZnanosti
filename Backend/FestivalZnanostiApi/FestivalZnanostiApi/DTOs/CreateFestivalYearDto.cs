﻿namespace FestivalZnanostiApi.DTOs
{
    public class CreateFestivalYearDto
    {
        public int Year { get; set; }
        public int Active { get; set; }
        public string Title { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        // public byte[]? Image { get; set; } SLIKA ĆE SE SLATI ODVOJENO NA FilesController
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ApplicationStart { get; set; }
        public DateTime? EditUntil { get; set; }
    }
}
