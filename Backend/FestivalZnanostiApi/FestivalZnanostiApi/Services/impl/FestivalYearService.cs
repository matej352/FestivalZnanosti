﻿using FestivalZnanostiApi.DTOs;
using FestivalZnanostiApi.DTOs.Extensions;
using FestivalZnanostiApi.Enums;
using FestivalZnanostiApi.Models;
using FestivalZnanostiApi.Repositories;
using FestivalZnanostiApi.Repositories.impl;
using FestivalZnanostiApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FestivalZnanostiApi.Servicess.impl
{
    public class FestivalYearService : IFestivalYearService
    {

        private readonly IFestivalYearRepository _repo;
        private readonly ITimeSlotService _timeSlotService;

        public FestivalYearService(IFestivalYearRepository repo, ITimeSlotService timeSlotService)
        {
            _repo = repo;
            _timeSlotService = timeSlotService;
        }

        public async Task<FestivalYearDto> CreateFestivalYear(CreateFestivalYearDto festivalYear)
        {
            try
            {

                var id = await _repo.CreateFestivalYear(festivalYear);


                // create timeslots for new year of festival
                await _timeSlotService.CreateTimeSlots(festivalYear.StartDate, festivalYear.EndDate, id);


                FestivalYear createdFestivalYear = _repo.FindById(id).Result;

                return createdFestivalYear.AsFestivalYearDto();

            }
            catch (Exception ex)
            {
                throw new Exception($"An problem occurred while creating new FestivalYear.");
            }

        }

        public Task<IEnumerable<FestivalYearDto>> GetFestivalYears()
        {
            return _repo.GetFestivalYears();
        }

        public async Task<FestivalYearDto> GetActiveFestivalYear()
        {
            return await _repo.FindActiveFestivalYear();
        }

        public Task<FestivalYearDto> GetFestivalYear(int festivalYearId)
        {
            return _repo.GetFestivalYear(festivalYearId);
        }

        public async Task<FestivalYearDto> UpdateFestivalYear(UpdateFestivalYearDto updateFestivalYearDto)
        {
            var id = await _repo.UpdateFestivalYear(updateFestivalYearDto);

            var _fy = await _repo.GetFestivalYear(id);

            return _fy;
        }

        public async Task ChangeFestivalYearActiveStatus(int id, FestivalYearActivityStatus active)
        {
            await _repo.ChangeFestivalYearActiveStatus(id, active);
        }
    }
}
