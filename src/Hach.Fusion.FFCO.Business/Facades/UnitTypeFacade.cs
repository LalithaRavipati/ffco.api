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
using Hach.Fusion.FFCO.Core.Dtos;
using Hach.Fusion.FFCO.Core.Entities;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the Unit Type repository. 
    /// </summary>    
    public class UnitTypeFacade
        : FacadeWithCruModelsBase<UnitTypeCommandDto, UnitTypeCommandDto, UnitTypeQueryDto, Guid>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for the <see cref="UnitTypeFacade"/> class taking a database context
        /// and validator argument.
        /// </summary>
        /// <param name="context">Database context containing Unit type entities.</param>
        /// <param name="validator">Validator for Unit Type DTOs.</param>
        public UnitTypeFacade(DataContext context, IFFValidator<UnitTypeCommandDto> validator)
        {
            _context = context;

            ValidatorCreate = validator;
            ValidatorUpdate = validator;

            _mapper = MappingManager.AutoMapper;
        }

        #region Get Methods

        /// <summary>
        /// Gets a list of Unit Types from the data store.
        /// </summary>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the list of Unit Type DTOs retrieved.
        /// </returns>
        public override async Task<QueryResult<UnitTypeQueryDto>> Get(ODataQueryOptions<UnitTypeQueryDto> queryOptions)
        {
            queryOptions.Validate(ValidationSettings);

            var results = await Task.Run(() => _context.UnitTypes
                .Select(_mapper.Map<UnitType, UnitTypeQueryDto>)
                .AsQueryable())
                .ConfigureAwait(false);

            return Query.Result(results);
        }

        /// <summary>
        /// Gets a single UnitType from the data store.
        /// </summary>
        /// <param name="id">ID that uniquely identifies the Unit Type to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result includes the Unit Type DTO retrieved.
        /// </returns>
        public override async Task<QueryResult<UnitTypeQueryDto>> Get(Guid id)
        {
            var result = await Task.Run(() => _context.UnitTypes
                .FirstOrDefault(l => l.Id == id))
                .ConfigureAwait(false);

            if (result == null)
                return Query.Error(EntityErrorCode.EntityNotFound);

            var dto = _mapper.Map<UnitType, UnitTypeQueryDto>(result);

            return Query.Result(dto);
        }

        #endregion Get Methods

        #region Not Implemented Methods

        public override Task<QueryResult<UnitTypeQueryDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<UnitTypeQueryDto, Guid>> Create(UnitTypeCommandDto dto)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<UnitTypeQueryDto, Guid>> Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<UnitTypeCommandDto, Guid>> Update(Guid id, Delta<UnitTypeCommandDto> delta)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<UnitTypeQueryDto, Guid>> CreateReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<UnitTypeQueryDto, Guid>> DeleteReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods
    }
}
