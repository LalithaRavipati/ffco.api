using AutoMapper;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.Data.Entities;
using Hach.Fusion.Data.Mapping;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Query;


namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the Location Type repository. 
    /// </summary>    
    public class LocationTypeFacade
        : FacadeWithCruModelsBase<LocationTypeBaseDto, LocationTypeBaseDto, LocationTypeQueryDto, Guid>,
          IFacadeWithCruModels<LocationTypeBaseDto, LocationTypeBaseDto, LocationTypeQueryDto, Guid>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for the <see cref="LocationTypeFacade"/> class taking a database context
        /// and validator argument.
        /// </summary>
        /// <param name="context">Database context containing Location Type entities.</param>
        /// <param name="validator">Validator for Location Type DTOs.</param>
        public LocationTypeFacade(DataContext context, IFFValidator<LocationTypeBaseDto> validator)
        {
            _context = context;
            _context.Configuration.LazyLoadingEnabled = false;
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
            // Thread.CurrentPrincipal is not available in the constructor.  Do not try to move this.
            var uid = GetCurrentUser();

            if (!uid.HasValue)
                return Query.Error(GeneralErrorCodes.TokenInvalid("UserId"));

            queryOptions.Validate(ValidationSettings);

            var results = await Task.Run(() => _context.LocationTypes
                .Include(x => x.LocationTypeGroup)
                .Include(x => x.LocationTypes)
                .Include(x => x.Parent)
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
            // Thread.CurrentPrincipal is not available in the constructor.  Do not try to move this.
            var uid = GetCurrentUser();

            if (!uid.HasValue)
                return Query.Error(GeneralErrorCodes.TokenInvalid("UserId"));

            var result = await Task.Run(() => _context.LocationTypes
                .Include(x => x.LocationTypeGroup)
                .Include(x => x.LocationTypes)
                .Include(x => x.Parent)
                .FirstOrDefault(l => l.Id == id))
                .ConfigureAwait(false);

            if (result == null)
                return Query.Error(EntityErrorCode.EntityNotFound);

            var locationTypeDto = _mapper.Map<LocationType, LocationTypeQueryDto>(result);

            return Query.Result(locationTypeDto);
        }

        #endregion Get Methods

        #region Utility Methods

        /// <summary>
        /// Gets the GUID User ID for the current user.
        /// </summary>
        /// <returns>The nullable GUID for the current user.</returns>
        private static Guid? GetCurrentUser()
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();

            return userId == null ? null : new Guid?(Guid.Parse(userId));
        }

        #endregion Methods

        #region Not Implemented Methods

        public override Task<CommandResult<LocationTypeQueryDto, Guid>> Create(LocationTypeBaseDto dto)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<LocationTypeQueryDto, Guid>> Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<LocationTypeBaseDto, Guid>> Update(Guid id,
            Delta<LocationTypeBaseDto> delta)
        {
            throw new NotImplementedException();
        }

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
