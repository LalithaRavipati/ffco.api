using Hach.Fusion.FFCO.Dtos;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Query;
using AutoMapper;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Business.Extensions;
using Hach.Fusion.FFCO.Business.Validators;
using Hach.Fusion.FFCO.Entities;
using Hach.Fusion.FFCO.Entities.Extensions;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the location repository. 
    /// </summary>    
    public class LocationFacade
        : FacadeWithCruModelsBase<LocationCommandDto, LocationCommandDto, LocationQueryDto, Guid>
    {
        private readonly DataContext _context;

        private readonly IMapper _mapper;       

        /// <summary>
        /// Constructor for the <see cref="LocationFacade"/> class taking a database context
        /// and validator argument.
        /// </summary>
        /// <param name="context">Database context containing location type entities.</param>
        /// <param name="validator">Validator for location DTOs.</param>
        public LocationFacade(DataContext context, IFFValidator<LocationCommandDto> validator)
        {
            _context = context;

            ValidatorCreate = validator;
            ValidatorUpdate = validator;

            _mapper = MappingManager.AutoMapper;            
        }

        #region Get Methods

        /// <summary>
        /// Gets a list of locations from the data store.
        /// </summary>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the list of location DTOs retrieved.
        /// </returns>
        public override async Task<QueryResult<LocationQueryDto>> Get(ODataQueryOptions<LocationQueryDto> queryOptions)
        {
            queryOptions.Validate(ValidationSettings);

            var results = await Task.Run(() => _context.ExpandedLocations()
                .Select(_mapper.Map<Location, LocationQueryDto>)
                .AsQueryable())
                .ConfigureAwait(false);
            
            return Query.Result(results);
        }

        /// <summary>
        /// Gets a single location from the data store.
        /// </summary>
        /// <param name="id">ID that uniquely identifies the location to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result includes the location DTO retrieved.
        /// </returns>
        public override async Task<QueryResult<LocationQueryDto>> Get(Guid id)
        {            
            var result = await Task.Run(() => _context.ExpandedLocations()
                .FirstOrDefault(l => l.Id == id))
                .ConfigureAwait(false);

            if (result == null)
                return Query.Error(EntityErrorCode.EntityNotFound);

            var locationDto = _mapper.Map<Location, LocationQueryDto>(result);

            return Query.Result(locationDto);
        }

        #endregion Get Methods

        #region Create Method

        /// <summary>
        /// Creates a location.
        /// </summary>
        /// <param name="dto">Data Transfer Object (DTO) used to create a location.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the DTO associated with the location.
        /// </returns>
        /// <remarks>
        /// Note that there is no checking for a circular reference for creating a location. This is
        /// because the created item does not have children. So, there cannot be a circular reference.
        /// </remarks>
        public override async Task<CommandResult<LocationQueryDto, Guid>> Create(LocationCommandDto dto)
        {
            // Thread.CurrentPrincipal is not available in the constrtor.  Do not try and move this
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();

            // User ID should always be available, but if not ...
            if (userId == null)
                return Command.Error<LocationQueryDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            var validationResponse = ValidatorCreate.Validate(dto);

            if (dto == null)
                return Command.Error<LocationQueryDto>(validationResponse);

            if (dto.Id != Guid.Empty)
                validationResponse.FFErrors.Add(ValidationErrorCode.PropertyIsInvalid(nameof(Location.Id)));

            var existingTask = _context.Locations.AnyAsync(l => l.Name == dto.Name);

            if (existingTask.Result)
                validationResponse.FFErrors.Add(EntityErrorCode.EntityAlreadyExists);

            if (dto.ParentId.HasValue)
            {
                existingTask = _context.Locations.AnyAsync(l => l.Id == dto.ParentId.Value);

                if (!existingTask.Result)
                    validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist(nameof(Location.ParentId)));
            }

            if (dto.LocationTypeId != Guid.Empty && !_context.LocationTypes.Any(lt => lt.Id == dto.LocationTypeId))
                validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist(nameof(Location.LocationTypeId)));

            if (validationResponse.IsInvalid)
                return Command.Error<LocationQueryDto>(validationResponse);

            var location = new Location
            {
                Id = Guid.NewGuid()
            };

            _mapper.Map(dto, location);

            location.SetAuditFieldsOnCreate(Guid.Parse(userId));
            
            _context.Locations.Add(location);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.Created(_mapper.Map(location, new LocationQueryDto()), location.Id);
        }

        #endregion Create Method

        #region Delete Method

        /// <summary>
        /// Deletes a location.
        /// </summary>
        /// <param name="id">ID that identifies the location to be deleted.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override async Task<CommandResult<LocationQueryDto, Guid>> Delete(Guid id)
        {
            // Thread.CurrentPrincipal is not available in the constructor.  Do not try and move this
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();

            var location = await _context.Locations
              .Include(l => l.Locations)
              .SingleOrDefaultAsync(l => l.Id == id)
              .ConfigureAwait(false);

            if (location == null)
                return Command.Error<LocationQueryDto>(EntityErrorCode.EntityNotFound);

            if (location.Locations.Count > 0)
                return Command.Error<LocationQueryDto>(EntityErrorCode.EntityCouldNotBeDeleted);

            _context.Locations.Attach(location);
            location.SetAuditFieldsOnUpdate(userId);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<LocationQueryDto>();
        }

        #endregion Delete Method

        #region Update Method

        /// <summary>
        /// Updates a location using a <see cref="Delta"/> object.
        /// </summary>
        /// <param name="id">ID of the location to be updated.</param>
        /// <param name="delta">
        /// Delta containing a list of location properties.  Web Api does the magic of converting the JSON to 
        /// a delta.
        /// </param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override async Task<CommandResult<LocationCommandDto, Guid>> Update(Guid id, Delta<LocationCommandDto> delta)
        {
            // Thread.CurrentPrincipal is not available in the constrtor.  Do not try and move this
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();

            // User ID should always be available, but if not ...
            if (userId == null)
                return Command.Error<LocationCommandDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            if (delta == null)
                return Command.Error<LocationCommandDto>(EntityErrorCode.EntityFormatIsInvalid);

            var location = await _context.Locations
                .SingleOrDefaultAsync(l => l.Id == id)
                .ConfigureAwait(false);

            if (location == null)
                return Command.Error<LocationCommandDto>(EntityErrorCode.EntityNotFound);

            var locationDto = _mapper.Map(location, new LocationCommandDto());
            delta.Patch(locationDto);

            var validationResponse = ValidatorUpdate.Validate(locationDto);

            if (locationDto.ParentId.HasValue)
            {
                var existingTask = _context.Locations.AnyAsync(l => l.Id == locationDto.ParentId.Value);

                if (!existingTask.Result)
                {
                    validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist(nameof(Location.ParentId)));
                }
                else
                {
                    // Check for circular references
                    if (await LocationValidator.IsCircularReference(_context.Locations, locationDto, id))
                        validationResponse.FFErrors.Add(ValidationErrorCode.CircularReferenceNotAllowed(nameof(Location.ParentId)));
                }
            }

            // Check that Location Type exists
            if (locationDto.LocationTypeId != Guid.Empty && !_context.LocationTypes.Any(lt => lt.Id == locationDto.LocationTypeId))
                validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist(nameof(Location.LocationTypeId)));

            // Including the original Id in the Patch request will not return an error but attempting to change the Id is not allowed.
            if (locationDto.Id != id)
                validationResponse.FFErrors.Add(ValidationErrorCode.EntityIDUpdateNotAllowed("Id"));

            // Check that unique fields are still unique            
            if (_context.Locations.Any(l => l.Id != id && l.Name == locationDto.Name))
                validationResponse.FFErrors.Add(ValidationErrorCode.EntityPropertyDuplicateNotAllowed(nameof(Location.Name)));

            if (validationResponse.IsInvalid)
                return Command.Error<LocationCommandDto>(validationResponse);

            _context.Locations.Attach(location);
            _mapper.Map(locationDto, location);

            location.SetAuditFieldsOnUpdate(userId);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<LocationCommandDto>();
        }

        #endregion Update Method

        #region Not Implemented Methods

        public override Task<QueryResult<LocationQueryDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<LocationQueryDto, Guid>> CreateReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<LocationQueryDto, Guid>> DeleteReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods
    }
}
