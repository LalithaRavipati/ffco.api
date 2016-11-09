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
using Hach.Fusion.FFCO.Core.Dtos;
using Hach.Fusion.FFCO.Core.Entities;
using Hach.Fusion.FFCO.Core.Extensions;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the Location Type repository. 
    /// </summary>    
    public class LocationTypeFacade
        : FacadeWithCruModelsBase<LocationTypeCommandDto, LocationTypeCommandDto, LocationTypeQueryDto, Guid>
    {
        private readonly DataContext _context;

        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for the <see cref="LocationTypeFacade"/> class taking a database context
        /// and validator argument.
        /// </summary>
        /// <param name="context">Database context containing Location Type entities.</param>
        /// <param name="validator">Validator for Location Type DTOs.</param>
        public LocationTypeFacade(DataContext context, IFFValidator<LocationTypeCommandDto> validator)
        {
            _context = context;

            ValidatorCreate = validator;
            ValidatorUpdate = validator;

            _mapper = MappingManager.AutoMapper;
        }

        #region Get Methods

        /// <summary>
        /// Gets a list of Location Types from the data store.
        /// </summary>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the list of Location Type DTOs retrieved.
        /// </returns>
        public override async Task<QueryResult<LocationTypeQueryDto>> Get(ODataQueryOptions<LocationTypeQueryDto> queryOptions)
        {
            queryOptions.Validate(ValidationSettings);

            var results = await Task.Run(() => _context.LocationTypes
                .Select(_mapper.Map<LocationType, LocationTypeQueryDto>)
                .AsQueryable())
                .ConfigureAwait(false);

            return Query.Result(results);
        }

        /// <summary>
        /// Gets a single Location Type from the data store.
        /// </summary>
        /// <param name="id">ID that uniquely identifies the Location Type to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result includes the Location Type DTO retrieved.
        /// </returns>
        public override async Task<QueryResult<LocationTypeQueryDto>> Get(Guid id)
        {
            var result = await Task.Run(() => _context.LocationTypes
                .FirstOrDefault(l => l.Id == id))
                .ConfigureAwait(false);

            if (result == null)
                return Query.Error(EntityErrorCode.EntityNotFound);

            var locationTypeDto = _mapper.Map<LocationType, LocationTypeQueryDto>(result);

            return Query.Result(locationTypeDto);
        }

        #endregion Get Methods

        #region Create Method

        /// <summary>
        /// Creates a Location Type.
        /// </summary>
        /// <param name="dto">Data Transfer Object (DTO) used to create a Location Type.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the DTO associated with the Location Type.
        /// </returns>
        public override async Task<CommandResult<LocationTypeQueryDto, Guid>> Create(LocationTypeCommandDto dto)
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            
            // User ID should always be available, but if not ...
            if (userId == null)
                return Command.Error<LocationTypeQueryDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            var validationResponse = ValidatorCreate.Validate(dto);

            if (dto == null)
                return Command.Error<LocationTypeQueryDto>(validationResponse);

            if (dto.Id != Guid.Empty)
                validationResponse.FFErrors.Add(ValidationErrorCode.PropertyIsInvalid(nameof(LocationType.Id)));

            var existingTask = _context.LocationTypes.AnyAsync(l => l.I18NKeyName == dto.I18NKeyName);

            if (existingTask.Result)
                validationResponse.FFErrors.Add(EntityErrorCode.EntityAlreadyExists);

            if (validationResponse.IsInvalid)
                return Command.Error<LocationTypeQueryDto>(validationResponse);

            var locationType = new LocationType
            {
                Id = Guid.NewGuid()
            };

            _mapper.Map(dto, locationType);

            locationType.SetAuditFieldsOnCreate(userId);
            
            _context.LocationTypes.Add(locationType);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.Created(_mapper.Map(locationType, new LocationTypeQueryDto()), locationType.Id);
        }

        #endregion Create Method

        #region Delete Method

        /// <summary>
        /// Deletes a Location Type if no Locations are associated with it.
        /// </summary>
        /// <param name="id">ID that identifies the Location Type to be deleted.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override async Task<CommandResult<LocationTypeQueryDto, Guid>> Delete(Guid id)
        {
            // Thread.CurrentPrincipal is not available in the constructor.  Do not try and move this
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();

            var locationType = await _context.LocationTypes                           
              .SingleOrDefaultAsync(l => l.Id == id)
              .ConfigureAwait(false);

            if (locationType == null)
                return Command.Error<LocationTypeQueryDto>(EntityErrorCode.EntityNotFound);

            var cannotDelete = _context.Locations
                .Any(x => x.LocationTypeId == id);
               
            if (cannotDelete)
                return Command.Error<LocationTypeQueryDto>(EntityErrorCode.EntityCouldNotBeDeleted);

            _context.LocationTypes.Remove(locationType);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<LocationTypeQueryDto>();
        }

        #endregion Delete Method

        #region Update Method

        /// <summary>
        /// Updates a Location Type using a <see cref="Delta"/> object.
        /// </summary>
        /// <param name="id">ID of the Location Type to be updated.</param>
        /// <param name="delta">
        /// Delta containing a list of Location Type properties.  Web Api does the magic of converting the JSON to 
        /// a delta.
        /// </param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override async Task<CommandResult<LocationTypeCommandDto, Guid>> Update(Guid id, Delta<LocationTypeCommandDto> delta)
        {
           
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            
            // User ID should always be available, but if not ...
            if (userId == null)
                return Command.Error<LocationTypeCommandDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            if (delta == null)
                return Command.Error<LocationTypeCommandDto>(EntityErrorCode.EntityFormatIsInvalid);

            var locationType = await _context.LocationTypes
                .SingleOrDefaultAsync(l => l.Id == id)
                .ConfigureAwait(false);

            if (locationType == null)
                return Command.Error<LocationTypeCommandDto>(EntityErrorCode.EntityNotFound);

            var dto = _mapper.Map(locationType, new LocationTypeCommandDto());
            delta.Patch(dto);

            var validationResponse = ValidatorUpdate.Validate(dto);

            // Including the original Id in the Patch request will not return an error but attempting to change the Id is not allowed.
            if (dto.Id != id)
                validationResponse.FFErrors.Add(ValidationErrorCode.EntityIDUpdateNotAllowed("Id"));

            // Check that unique fields are still unique            
            if (_context.LocationTypes.Any(l => l.Id != id && l.I18NKeyName == dto.I18NKeyName))
                validationResponse.FFErrors.Add(ValidationErrorCode.EntityPropertyDuplicateNotAllowed(nameof(LocationType.I18NKeyName)));

            if (validationResponse.IsInvalid)
                return Command.Error<LocationTypeCommandDto>(validationResponse);

            _context.LocationTypes.Attach(locationType);
            _mapper.Map(dto, locationType);

            locationType.SetAuditFieldsOnUpdate(userId);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<LocationTypeCommandDto>();
        }

        #endregion Update Method

        #region Not Implemented Methods

        /// <summary>
        /// Gets the value of the indicated Location Type's property (not implemented).
        /// </summary>
        /// <param name="id">ID that identifies the Location Type to be retrieved.</param>
        /// <param name="propertyName">Name of the property whose value is to be retrieved.</param>
        /// <returns>
        /// Always throws a NotImplemented exception.
        /// </returns>
        public override Task<QueryResult<LocationTypeQueryDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<LocationTypeQueryDto, Guid>> CreateReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<LocationTypeQueryDto, Guid>> DeleteReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods
    }
}
