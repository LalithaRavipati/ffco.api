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
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.Data.Entities;
using Hach.Fusion.Data.Mapping;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the ChemicalFormTypes repository. 
    /// </summary>    
    public class ChemicalFormTypesFacade
        : FacadeWithCruModelsBase<ChemicalFormTypeBaseDto, ChemicalFormTypeBaseDto, ChemicalFormTypeQueryDto, Guid>,
         IFacadeWithCruModels<ChemicalFormTypeBaseDto, ChemicalFormTypeBaseDto, ChemicalFormTypeQueryDto, Guid>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for the <see cref="ChemicalFormTypesFacade"/> class taking a database context
        /// and validator argument.
        /// </summary>
        /// <param name="context">Database context containing Chemical Form type entities.</param>
        public ChemicalFormTypesFacade(DataContext context)
        {
            _context = context;
            _mapper = MappingManager.AutoMapper;
        }

        #region Get Methods

        /// <summary>
        /// Gets a list of chemical form types from the data store.
        /// </summary>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the list of DTOs retrieved.
        /// </returns>
        public override async Task<QueryResult<ChemicalFormTypeQueryDto>> Get(ODataQueryOptions<ChemicalFormTypeQueryDto> queryOptions)
        {
            queryOptions.Validate(ValidationSettings);

            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Query.Error(GeneralErrorCodes.TokenInvalid("UserId"));

            var result = await Task.Run(() =>
                _context.ChemicalFormTypes
                .Select(_mapper.Map<ChemicalFormType, ChemicalFormTypeQueryDto>)
                .AsQueryable())
                .ConfigureAwait(false);

            return Query.Result(result);
        }

        /// <summary>
        /// Gets a single chemical form types from the data store.
        /// </summary>
        /// <param name="id">ID that uniquely identifies the entity to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result includes the DTO retrieved.
        /// </returns>
        public override async Task<QueryResult<ChemicalFormTypeQueryDto>> Get(Guid id)
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Query.Error(GeneralErrorCodes.TokenInvalid("UserId"));

            var result = await _context.ChemicalFormTypes
                .FirstOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            return result == null
                ? Query.Error(EntityErrorCode.EntityNotFound)
                : Query.Result(_mapper.Map<ChemicalFormType, ChemicalFormTypeQueryDto>(result));
        }

        #endregion Get Methods

        #region Not Implemented Methods
        #region Create Method

        /// <summary>
        /// Creates a ChemicalForm Type.
        /// </summary>
        /// <param name="dto">Data Transfer Object (DTO) used to create an entity.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the DTO associated with the entity created.
        /// </returns>
        public override Task<CommandResult<ChemicalFormTypeQueryDto, Guid>> Create(ChemicalFormTypeBaseDto dto)
        {
            throw new NotImplementedException();
        }

        #endregion Create Method

        #region Delete Method

        /// <summary>
        /// Deletes a ChemicalForm Type.
        /// </summary>
        /// <param name="id">ID that identifies the entity to be deleted.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override Task<CommandResult<ChemicalFormTypeQueryDto, Guid>> Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        #endregion Delete Method

        #region Update Method

        /// <summary>
        /// Updates a ChemicalForm using a <see cref="Delta"/> object.
        /// </summary>
        /// <param name="id">ID of the entity to be updated.</param>
        /// <param name="delta">
        /// Delta containing a list of entity properties.  Web Api does the magic of converting the JSON to 
        /// a delta.
        /// </param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override Task<CommandResult<ChemicalFormTypeBaseDto, Guid>> Update(Guid id, Delta<ChemicalFormTypeBaseDto> delta)
        {
            throw new NotImplementedException();
        }

        #endregion Update Method

        public override Task<QueryResult<ChemicalFormTypeQueryDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<ChemicalFormTypeQueryDto, Guid>> CreateReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<ChemicalFormTypeQueryDto, Guid>> DeleteReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods
    }
}
