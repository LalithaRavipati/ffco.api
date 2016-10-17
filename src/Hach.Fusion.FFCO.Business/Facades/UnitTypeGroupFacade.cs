using Hach.Fusion.FFCO.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Query;
using AutoMapper;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Business.Extensions;
using Hach.Fusion.FFCO.Entities;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the Unit Type Group repository. 
    /// </summary>    
    public class UnitTypeGroupFacade
        : FacadeWithCruModelsBase<UnitTypeGroupQueryDto, UnitTypeGroupQueryDto, UnitTypeGroupQueryDto, Guid>
    {
        private readonly DataContext _context;

        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for the <see cref="LocationFacade"/> class taking a database context
        /// and validator argument.
        /// </summary>
        /// <param name="context">Database context containing Unit Type Group type entities.</param>
        /// <param name="validator">Validator for Unit Type Group DTOs.</param>
        public UnitTypeGroupFacade(DataContext context, IFFValidator<UnitTypeGroupQueryDto> validator)
        {
            _context = context;

            ValidatorCreate = validator;
            ValidatorUpdate = validator;

            _mapper = MappingManager.AutoMapper;
        }

        #region Get Methods

        /// <summary>
        /// Gets a list of Unit Type Groups from the data store.
        /// </summary>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the list of Unit Type Groups DTOs retrieved.
        /// </returns>
        public override async Task<QueryResult<UnitTypeGroupQueryDto>> Get(ODataQueryOptions<UnitTypeGroupQueryDto> queryOptions)
        {
            queryOptions.Validate(ValidationSettings);

            var results = await Task.Run(() => _context.UnitTypeGroups
                .Select(_mapper.Map<UnitTypeGroup, UnitTypeGroupQueryDto>)
                .AsQueryable())
                .ConfigureAwait(false);

            return Query.Result(results);
        }

        /// <summary>
        /// Gets a single Unit Type Group from the data store.
        /// </summary>
        /// <param name="id">ID that uniquely identifies the Unit Type Group to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result includes the Unit Type Group DTO retrieved.
        /// </returns>
        public override async Task<QueryResult<UnitTypeGroupQueryDto>> Get(Guid id)
        { 
            var result = await Task.Run(() => _context.UnitTypeGroups
                .FirstOrDefault(l => l.Id == id))
                .ConfigureAwait(false);

            if (result == null)
                return Query.Error(EntityErrorCode.EntityNotFound);

            var dto = _mapper.Map<UnitTypeGroup, UnitTypeGroupQueryDto>(result);

            return Query.Result(dto);
        }

        /// <summary>
        /// Gets the value of the indicated location's property.
        /// </summary>
        /// <param name="id">ID that identifies the location to be retrieved.</param>
        /// <param name="propertyName">Name of the property whose value is to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the indicated property's value.
        /// </returns>
        public override Task<QueryResult<UnitTypeGroupQueryDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();

            /*var result = await _context.Locations
                .SingleOrDefaultAsync(l => l.Id == id)
                .ConfigureAwait(false);

            if (result == null)
                return Query.Error(EntityErrorCode.EntityNotFound);

            var resultDto = Mapper.Map<Location, LocationQueryDto>(result);

            var property = resultDto.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

            if (property == null)
                return Query.Error(EntityErrorCode.EntityPropertyNotFound);

            var value = property.GetValue(resultDto);

            string valueString;
            if (property.Name == "Locations" || property.Name == "Point")
                valueString = JsonConvert.SerializeObject(value);
            else if (property.PropertyType == typeof(DateTime))
                valueString = ((DateTime)value).ToString("s");
            else
                valueString = value.ToString();

            return new QueryResult<LocationQueryDto>(valueString);*/
        }

        #endregion Get Methods

        #region Not Implemented Methods

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
        public override Task<CommandResult<UnitTypeGroupQueryDto, Guid>> Create(UnitTypeGroupQueryDto dto)
        {
            throw new NotImplementedException();

            /*//TODO: RFKutz, 09/10/2016
            //var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            Guid? userId = null;
            // User ID should always be available, but if not ...
            if (userId == null)
                return Command.Error<LocationQueryDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            var validationResponse = ValidatorCreate.Validate(dto);

            if (dto == null)
                return Command.Error<LocationQueryDto>(validationResponse);

            if (dto.Id != Guid.Empty)
                validationResponse.FFErrors.Add(ValidationErrorCode.PropertyIsInvalid("Id"));

            var existingTask = _context.Locations.AnyAsync(l => l.Name == dto.Name);

            if (existingTask.Result)
                validationResponse.FFErrors.Add(EntityErrorCode.EntityAlreadyExists);

            if (dto.ParentId.HasValue)
            {
                existingTask = _context.Locations.AnyAsync(l => l.Id == dto.ParentId.Value);

                if (!existingTask.Result)
                    validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("ParentId"));
            }

            //if (dto.UnitTypeGroupId != Guid.Empty && !_context.UnitTypeGroups.Any(lt => lt.Id == dto.UnitTypeGroupId))
            //    validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("UnitTypeGroupId"));

            if (validationResponse.IsInvalid)
                return Command.Error<LocationQueryDto>(validationResponse);

            var location = new Location
            {
                Id = Guid.NewGuid()
            };

            Mapper.Map(dto, location);

            //TODO: RFKutz, 09/10/2016
            //location.CreatedById = Guid.Parse(userId);
            location.CreatedById = Guid.NewGuid();
            location.CreatedOn = DateTime.UtcNow;
            location.ModifiedById = location.CreatedById;
            location.ModifiedOn = location.CreatedOn;

            _context.Locations.Add(location);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.Created(Mapper.Map(location, new LocationQueryDto()), location.Id);*/
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
        public override Task<CommandResult<UnitTypeGroupQueryDto, Guid>> Delete(Guid id)
        {
            throw new NotImplementedException();

            /*var location = await _context.Locations
              .Include(l => l.ChildLocations)
              .SingleOrDefaultAsync(l => l.Id == id)
              .ConfigureAwait(false);

            if (location == null)
                return Command.Error<LocationQueryDto>(EntityErrorCode.EntityNotFound);

            if (location.ChildLocations.Count > 0)
                return Command.Error<LocationQueryDto>(EntityErrorCode.EntityCouldNotBeDeleted);

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<LocationQueryDto>();*/
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
        public override Task<CommandResult<UnitTypeGroupQueryDto, Guid>> Update(Guid id, Delta<UnitTypeGroupQueryDto> delta)
        {
            throw new NotImplementedException();

            /*//TODO: RFKutz [09/10/2016]
            //var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            Guid? userId = null;
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

            var locationDto = Mapper.Map(location, new LocationCommandDto());
            delta.Patch(locationDto);

            var validationResponse = ValidatorUpdate.Validate(locationDto);

            if (locationDto.ParentId.HasValue)
            {
                var existingTask = _context.Locations.AnyAsync(l => l.Id == locationDto.ParentId.Value);

                if (!existingTask.Result)
                {
                    validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("ParentId"));
                }
                else
                {
                    // Check for circular references
                    if (await LocationValidator.IsCircularReference(_context.Locations, locationDto, id))
                        validationResponse.FFErrors.Add(ValidationErrorCode.CircularReferenceNotAllowed("ParentId"));
                }
            }

            // Check that Location Type exists
            //if (locationDto.UnitTypeGroupId != Guid.Empty && !_context.UnitTypeGroups.Any(lt => lt.Id == locationDto.UnitTypeGroupId))
            //    validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("UnitTypeGroupId"));

            // Including the original Id in the Patch request will not return an error but attempting to change the Id is not allowed.
            if (locationDto.Id != id)
                validationResponse.FFErrors.Add(ValidationErrorCode.EntityIDUpdateNotAllowed("Id"));

            // Check that unique fields are still unique            
            if (_context.Locations.Any(l => l.Id != id && l.Name == locationDto.Name))
                validationResponse.FFErrors.Add(ValidationErrorCode.EntityPropertyDuplicateNotAllowed("InternalName"));

            if (validationResponse.IsInvalid)
                return Command.Error<LocationCommandDto>(validationResponse);

            _context.Locations.Attach(location);
            Mapper.Map(locationDto, location);

            //TODO: RFKutz [09/10/2016]
            //location.ModifiedById = Guid.Parse(userId);
            location.ModifiedById = Guid.NewGuid();
            location.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<LocationCommandDto>();*/
        }

        #endregion Update Method

        public override Task<CommandResult<UnitTypeGroupQueryDto, Guid>> CreateReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<UnitTypeGroupQueryDto, Guid>> DeleteReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods
    }
}
