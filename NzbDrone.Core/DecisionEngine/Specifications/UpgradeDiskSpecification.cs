using System;
using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeDiskSpecification : IDecisionEngineSpecification
    {
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public UpgradeDiskSpecification(QualityUpgradableSpecification qualityUpgradableSpecification, Logger logger)
        {
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Existing file on disk is of equal or higher quality";
            }
        }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                _logger.Trace("Comparing file quality with report. Existing file is {0}", file.Quality);

                if (!_qualityUpgradableSpecification.IsUpgradable(file.Quality, subject.ParsedEpisodeInfo.Quality))
                {
                    return false;
                }

                if (searchCriteria == null &&
                    subject.ParsedEpisodeInfo.Quality.Quality == file.Quality.Quality &&
                    subject.ParsedEpisodeInfo.Quality.Proper &&
                    file.DateAdded < DateTime.Today.AddDays(-7))
                {
                    _logger.Trace("Proper for old file, skipping: {0}", subject);
                    return false;
                }
            }

            return true;
        }
    }
}
