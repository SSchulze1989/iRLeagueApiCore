using iRLeagueApiCore.Common.Models.Reviews;

namespace iRLeagueApiCore.Client.Endpoints.VoteCategories;

public interface IVoteCategoriesEndpoint : IGetAllEndpoint<VoteCategoryModel>, IWithIdEndpoint<IVoteCategoryByIdEndpoint>
{
}
