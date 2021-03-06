﻿using System;
using System.Threading.Tasks;
using Hach.Fusion.Core.Business.Facades;
using System.Web.OData.Query;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Data.Dtos;

namespace Hach.Fusion.FFCO.Business.Facades
{
    public interface IInAppMessageFacade : IFacadeWithCruModels<InAppMessageBaseDto, InAppMessageBaseDto, InAppMessageQueryDto, Guid>
    {
        /// <summary>
        /// Retrieves InAppMessages for the specified User
        /// </summary>
        /// <param name="userId">Id of the User who's InAppMessages will be retrieved</param>
        /// <param name="options">OData Query Options</param>
        /// <returns>Queryable collection of InAppMessages for the specified User</returns>
        Task<QueryResult<InAppMessageQueryDto>> GetByUserId(Guid userId, ODataQueryOptions<InAppMessageQueryDto> options);
    }
}
