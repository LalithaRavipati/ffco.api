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
        /// Constructor for the <see cref="UnitTypeGroupFacade"/> class taking a database context
        /// and validator argument.
        /// </summary>
        /// <param name="context">Database context containing Unit Type Group entities.</param>
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

        #endregion Get Methods

        #region Not Implemented Methods

        /// <summary>
        /// Gets the value of the indicated Unit Type Group's property (not implemented).
        /// </summary>
        /// <param name="id">ID that identifies the Unit Type Group to be retrieved.</param>
        /// <param name="propertyName">Name of the property whose value is to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the indicated property's value.
        /// </returns>
        public override Task<QueryResult<UnitTypeGroupQueryDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();
        }

        #region Create Method

        /// <summary>
        /// Creates a Unit Type Group (not implemented).
        /// </summary>
        /// <param name="dto">Data Transfer Object (DTO) used to create a Unit Type Group.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the DTO associated with the Unit Type Group.
        /// </returns>
        public override Task<CommandResult<UnitTypeGroupQueryDto, Guid>> Create(UnitTypeGroupQueryDto dto)
        {
            throw new NotImplementedException();
        }

        #endregion Create Method

        #region Delete Method

        /// <summary>
        /// Deletes a Unit Type Group (not implemented).
        /// </summary>
        /// <param name="id">ID that identifies the Unit Type Group to be deleted.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override Task<CommandResult<UnitTypeGroupQueryDto, Guid>> Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        #endregion Delete Method

        #region Update Method

        /// <summary>
        /// Updates a Unit Type Group using a <see cref="Delta"/> object.
        /// </summary>
        /// <param name="id">ID of the Unit Type Group to be updated.</param>
        /// <param name="delta">
        /// Delta containing a list of Unit Type Group properties.  Web Api does the magic of converting the JSON to 
        /// a delta.
        /// </param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override Task<CommandResult<UnitTypeGroupQueryDto, Guid>> Update(Guid id, Delta<UnitTypeGroupQueryDto> delta)
        {
            throw new NotImplementedException();
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
