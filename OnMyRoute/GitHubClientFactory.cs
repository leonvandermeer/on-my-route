using Microsoft.Extensions.Options;
using Octokit;
using Updates.Updates;

namespace OnMyRoute;

class GitHubClientFactory(IOptions<UpdateData> updateData) {
    private readonly UpdateData updateData = updateData.Value;

    public IGitHubClient Create() =>
        new GitHubClient(
            new ProductHeaderValue(
                updateData.Product.Replace(' ', '_'),
                updateData.CurrentVersion
            )
        );
}
