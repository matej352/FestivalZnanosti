﻿using FestivalZnanostiApi.DTOs;
using FestivalZnanostiApi.Enums;
using FestivalZnanostiApi.Models;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;

namespace FestivalZnanostiApi.Repositories.impl
{
    public class EventsRepository : IEventsRepository
    {


        private readonly FestivalZnanostiContext _context;


        public EventsRepository(FestivalZnanostiContext context)
        {
            _context = context;
        }

        public async Task<Event> GetEvent(int id)
        {
            var _event = await _context.Event
                .Include(e => e.Location)
                .Include(e => e.Location.ParentLocation)
                .Include(e => e.Submitter)
                .Include(e => e.ParticipantsAge)
                .Include(e => e.Lecturer)
                .Include(e => e.TimeSlot)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (_event is null)
            {
                throw new Exception($"Event with id = {id} does not exists");
            }
            else
            {
                return _event;
            }
        }

        public async Task<IEnumerable<Event>> GetEvents()
        {
            var events = await _context.Event
                .Include(e => e.Location)
                .Include(e => e.Location.ParentLocation)
                .Include(e => e.Submitter)
                .Include(e => e.ParticipantsAge)
                .Include(e => e.Lecturer)
                .Include(e => e.TimeSlot)
                .ToListAsync();

            return events;
        }

        public async Task<IEnumerable<Event>> GetEventsForFestivalYear(int festivalYearId)
        {
            var events = await _context.Event
                .Include(e => e.Location)
                .Include(e => e.Location.ParentLocation)
                .Include(e => e.Submitter)
                .Include(e => e.ParticipantsAge)
                .Include(e => e.Lecturer)
                .Include(e => e.TimeSlot)
                .Where(e => e.FestivalYearId == festivalYearId)
                .ToListAsync();

            return events;
        }


        public async Task<IEnumerable<Event>> GetEvents(int submitterId)
        {
            var events = await _context.Event
                .Include(e => e.Location)
                .Include(e => e.Location.ParentLocation)
                .Include(e => e.Submitter)
                .Include(e => e.ParticipantsAge)
                .Include(e => e.Lecturer)
                .Include(e => e.TimeSlot)
                .Where(e => e.SubmitterId == submitterId)
                .ToListAsync();

            return events;
        }

        public async Task<int> SaveEvent(CreateEventDto createEvent, int submitterId)
        {

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    //TODO: Provjerit da je sve validirano, code refactor

                    //  VALIDATION - location with provided id exists
                    var location = _context.Location.Where(location => location.Id == createEvent.LocationId).FirstOrDefault();
                    if (location is null)
                    {
                        throw new Exception("Location with provided id does not exists!");
                    }
                    var timeSlotsTracked = location.TimeSlotsTracked;


                    //  VALIDATION - event timeslots are provided
                    if (createEvent.TimeSlotIds is null)
                    {
                        throw new Exception("Timeslots are required for creating Event!");
                    }


                    //get models from dtos
                    List<int> timeSlotIds = createEvent.TimeSlotIds;
                    List<TimeSlot> timeSlots = await _context.TimeSlot
                        .Where(ts => timeSlotIds.Contains(ts.Id))
                        .ToListAsync();

                    // Check are given timeslots timeslots for the proper location
                    if (!timeSlotsTracked)
                    {
                        bool allHaveNullLocationId = timeSlots.All(slot => slot.LocationId == null);
                        if (!allHaveNullLocationId)
                        {
                            throw new Exception("Not all timeslots correspond to the given event location!");
                        }
                    }
                    else
                    {
                        bool allHaveProperLocationId = timeSlots.All(slot => slot.LocationId == location.Id);
                        if (!allHaveProperLocationId)
                        {
                            throw new Exception("Not all timeslots correspond to the given event location!");
                        }
                    }



                    List<int> participantsAgesIds = createEvent.ParticipantsAges.Select(dto => dto.Id).ToList();
                    List<ParticipantsAge> participantsAges = await _context.ParticipantsAge
                        .Where(pa => participantsAgesIds.Contains(pa.Id))
                        .ToListAsync();


                    //  VALIDATION - That means that Event is in Tehnički muzej ( Kino dvorana (id=2), Izložbena dvorana (id=3) or Dvroište(id=4) )
                    if (timeSlotsTracked)
                    {
                        checkEventTypeAndLocationMatch(location.Id, createEvent.Type);

                        //  TODO: provjerit pripadaju li timeslotovi lokaciji sa createEvent.LocationId
                    }
                    else
                    {
                        //check are the selected timeslots actually timeslots for untrackable Events (TimeSlot.LocationId should be null)
                        bool allHaveNullLocationId = timeSlots.All(ts => ts.LocationId == null);

                        if (!allHaveNullLocationId)
                        {
                            throw new Exception("Timeslots do not correspond to the provided location!");
                        }
                    }



                    //Check are timeslots between startDate end endDate of currently active festival year ILI DA ROKNEM DA SVAKI TIMESLOT IMA ID FESTIVALA



                    // Create the Event
                    var newEvent = new Event
                    {
                        Title = createEvent.Title,
                        Status = (int)EventStatus.Pending,
                        Type = mapEventType(createEvent.Type),
                        VisitorsCount = createEvent.VisitorsCount,
                        Equipment = createEvent.Equipment,
                        Summary = createEvent.Summary,
                        LocationId = createEvent.LocationId,
                        FestivalYearId = _context.FestivalYear.Where(festivalYear => festivalYear.Active == 1).FirstOrDefault()!.Id,
                        SubmitterId = submitterId,
                        TimeSlot = (ICollection<TimeSlot>)timeSlots,
                        ParticipantsAge = (ICollection<ParticipantsAge>)participantsAges
                    };

                    _context.Event.Add(newEvent);
                    await _context.SaveChangesAsync();

                    // Create Lecturers
                    if (createEvent.Lecturers != null)
                    {
                        foreach (var lecturerDto in createEvent.Lecturers)
                        {
                            var newLecturer = new Lecturer
                            {
                                FirstName = lecturerDto.FirstName,
                                LastName = lecturerDto.LastName,
                                Phone = lecturerDto.Phone,
                                Email = lecturerDto.Email,
                                Type = (int)lecturerDto.Type == 1 ? true : false,
                                Resume = lecturerDto.Resume,
                                EventId = newEvent.Id
                            };

                            _context.Lecturer.Add(newLecturer);
                        }

                        await _context.SaveChangesAsync();
                    }



                    if (timeSlotsTracked && createEvent.Type != EventType.Izlozba)
                    {
                        await IncrementTimeSlotBookedCount(createEvent.TimeSlotIds, createEvent.LocationId, location.ParallelEventCount);
                    }

                    transaction.Commit();

                    return newEvent.Id;
                }
                catch (Exception exception)
                {
                    transaction.Rollback();
                    throw new Exception("Problem while creating new Event! \n" + exception.Message);
                }


            }


        }

        private void checkEventTypeAndLocationMatch(int locationId, EventType eventType)
        {
            if ((eventType == EventType.Predavanje && locationId == 2) || ((eventType == EventType.Prezentacija || eventType == EventType.Radionica) && (locationId == 3 || locationId == 4)) || (eventType == EventType.Izlozba && locationId == 3))
            {
                //everything ok
            }
            else
            {
                throw new Exception("Event type and location mismatch!");
            }
        }

        private string mapEventType(EventType type)
        {
            switch ((int)type)
            {
                case 0:
                    return "Predavanje";
                case 1:
                    return "Prezentacija";
                case 2:
                    return "Radionica";
                case 3:
                    return "Izložba";
                default:
                    throw new Exception("Invalid event type");
            }
        }

        public async Task<int> UpdateEvent(UpdateEventDto updateEvent)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    //  VALIDATION - location with provided id exists
                    var location = _context.Location.Where(location => location.Id == updateEvent.LocationId).FirstOrDefault();
                    if (location is null)
                    {
                        throw new Exception("Location with provided id does not exists!");
                    }
                    var timeSlotsTracked = location.TimeSlotsTracked;

                    //get models from dtos
                    List<int> timeSlotIds = updateEvent.TimeSlotIds;
                    List<TimeSlot> timeSlots = await _context.TimeSlot
                        .Where(ts => timeSlotIds.Contains(ts.Id))
                        .ToListAsync();




                    // Retrieve the existing event
                    var existingEvent = await _context.Event
                        .Include(e => e.TimeSlot)
                        .Include(e => e.ParticipantsAge)
                        .Include(e => e.Lecturer)
                        .FirstOrDefaultAsync(e => e.Id == updateEvent.Id);

                    if (existingEvent == null)
                    {
                        throw new Exception($"Event with ID {updateEvent.Id} not found.");
                    }

                    //  VALIDATION - That means that Event is in Tehnički muzej ( Kino dvorana (id=2), Izložbena dvorana (id=3) or Dvroište(id=4) )
                    if (timeSlotsTracked)
                    {
                        checkEventTypeAndLocationMatch(location.Id, updateEvent.Type);

                        //  TODO: provjerit pripadaju li timeslotovi lokaciji sa createEvent.LocationId
                    }
                    else
                    {
                        //check are the selected timeslots actually timeslots for untrackable Events (TimeSlot.LocationId should be null)
                        bool allHaveNullLocationId = timeSlots.All(ts => ts.LocationId == null);

                        if (!allHaveNullLocationId)
                        {
                            throw new Exception("Timeslots do not correspond to the provided location!");
                        }
                    }



                    // Update the existing event properties
                    existingEvent.Title = updateEvent.Title;
                    existingEvent.VisitorsCount = updateEvent.VisitorsCount;
                    existingEvent.Equipment = updateEvent.Equipment;
                    existingEvent.Summary = updateEvent.Summary;



                    var trackedLocationsIds = await _context.Location.Where(l => l.TimeSlotsTracked).Select(l => l.Id).ToListAsync();

                    if (existingEvent.Type != "Izložba" && trackedLocationsIds.Contains(existingEvent.LocationId))
                    {
                        await DecrementTimeSlotBookedCount(existingEvent.TimeSlot.Select(t => t.Id).ToList(), existingEvent.LocationId);
                    }

                    existingEvent.Type = mapEventType(updateEvent.Type);
                    existingEvent.LocationId = updateEvent.LocationId;


                    // Clear existing TimeSlots and add new ones
                    existingEvent.TimeSlot.Clear();



                    if (updateEvent.TimeSlotIds != null)
                    {

                        foreach (var timeSlot in timeSlots)
                        {
                            existingEvent.TimeSlot.Add(timeSlot);
                        }
                    }
                    else
                    {
                        throw new Exception("Timeslots are required for updating Event!");
                    }

                    if (timeSlotsTracked && updateEvent.Type != EventType.Izlozba)
                    {

                        await IncrementTimeSlotBookedCount(updateEvent.TimeSlotIds, updateEvent.LocationId, location.ParallelEventCount);

                    }



                    // Update ParticipantsAges

                    List<int> participantsAgesIds = updateEvent.ParticipantsAges.Select(dto => dto.Id).ToList();
                    List<ParticipantsAge> participantsAges = await _context.ParticipantsAge
                        .Where(pa => participantsAgesIds.Contains(pa.Id))
                        .ToListAsync();

                    existingEvent.ParticipantsAge.Clear();
                    existingEvent.ParticipantsAge = (ICollection<ParticipantsAge>)participantsAges;



                    //Update Lecturers

                    if (updateEvent.LecturersForDelete != null)
                    {
                        foreach (var id in updateEvent.LecturersForDelete)
                        {
                            var lecturer = await _context.Lecturer.FindAsync(id);

                            if (lecturer != null)
                            {
                                _context.Lecturer.Remove(lecturer);
                            }
                        }

                        await _context.SaveChangesAsync();
                    }

                    if (updateEvent.LecturersForCreate != null)
                    {
                        foreach (var lecturerDto in updateEvent.LecturersForCreate)
                        {
                            var newLecturer = new Lecturer
                            {
                                FirstName = lecturerDto.FirstName,
                                LastName = lecturerDto.LastName,
                                Phone = lecturerDto.Phone,
                                Email = lecturerDto.Email,
                                Type = (int)lecturerDto.Type == 1 ? true : false,
                                Resume = lecturerDto.Resume,
                                EventId = existingEvent.Id
                            };

                            _context.Lecturer.Add(newLecturer);
                        }

                        await _context.SaveChangesAsync();
                    }

                    if (updateEvent.LecturersForUpdate != null)
                    {
                        foreach (var lecturerDto in updateEvent.LecturersForUpdate)
                        {
                            var lecturer = await _context.Lecturer.FindAsync(lecturerDto.Id);

                            if (lecturer != null)
                            {
                                lecturer.FirstName = lecturerDto.FirstName;
                                lecturer.LastName = lecturerDto.LastName;
                                lecturer.Phone = lecturerDto.Phone;
                                lecturer.Email = lecturerDto.Email;
                                lecturer.Type = (int)lecturerDto.Type == 1 ? true : false;
                                lecturer.Resume = lecturerDto.Resume;

                                await _context.SaveChangesAsync();
                            }
                        }

                    }





                    await _context.SaveChangesAsync();

                    transaction.Commit();

                    return updateEvent.Id;
                }
                catch (Exception exception)
                {
                    transaction.Rollback();
                    throw new Exception($"Problem while updating Event with ID {updateEvent.Id}! \n" + exception.Message);
                }
            }
        }

        public async Task<Account> GetEventSubmitter(int id)
        {
            var _event = await _context.Event.Include(e => e.Submitter).Where(e => e.Id == id).FirstOrDefaultAsync();

            if (_event is null)
            {
                throw new Exception($"Event with id = {id} does not exists");
            }
            else
            {
                return _event.Submitter;
            }


        }

        public async Task DeleteEvent(int eventId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {

                    var dbEvent = await _context.Event.Include(e => e.TimeSlot).SingleOrDefaultAsync(e => e.Id == eventId);

                    if (dbEvent != null)
                    {

                        await DecrementTimeSlotBookedCount(dbEvent.TimeSlot.Select(t => t.Id).ToList(), dbEvent.LocationId);



                        _context.Event.Remove(dbEvent);
                        await _context.SaveChangesAsync();

                        transaction.Commit();
                    }
                    else
                    {
                        throw new Exception($"Event with id = {eventId} does not exist!");
                    }


                }
                catch (Exception exception)
                {
                    transaction.Rollback();
                    throw new Exception($"Problem while deleting Event with ID {eventId}! \n" + exception.Message);
                }
            }
        }



        /*
         
        Predavanje --> u kino dvorani
        Prezentacija ili Radionica --> Izložbena dvorana ili Dvorište
         
         
         
         */




        private async Task IncrementTimeSlotBookedCount(List<int> timeslotIds, int locationId, int parallelEventCount)
        {
            foreach (var timeSlotId in timeslotIds)
            {
                var foundTimeSlot = await _context.TimeSlot
                    .Where(ts => ts.Id == timeSlotId && ts.LocationId == locationId)
                    .FirstOrDefaultAsync();

                if (foundTimeSlot != null)
                {
                    foundTimeSlot.BookedCount += 1;

                    if (foundTimeSlot.BookedCount > parallelEventCount)
                    {
                        throw new Exception($"Time slot with id {foundTimeSlot.Id} is already full!");
                    }
                }
            }

            await _context.SaveChangesAsync();
        }


        private async Task DecrementTimeSlotBookedCount(List<int> timeslotIds, int locationId)
        {
            foreach (var timeSlotId in timeslotIds)
            {
                var foundTimeSlot = await _context.TimeSlot
                    .Where(ts => ts.Id == timeSlotId && ts.LocationId == locationId)
                    .FirstOrDefaultAsync();

                if (foundTimeSlot != null)
                {
                    foundTimeSlot.BookedCount -= 1;

                    if (foundTimeSlot.BookedCount < 0)
                    {
                        throw new Exception($"Time slot with id {foundTimeSlot.Id} can't have BookedCount lower than 0!");
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task ChangeStatus(int eventId, EventStatus status)
        {
            var ev = await _context.Event.FindAsync(eventId);
            if (ev != null)
            {
                ev.Status = (int)status;
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception($"Event with id {eventId} does not exist!");
            }
        }
    }
}
