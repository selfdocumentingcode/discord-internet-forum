using System;

namespace DifBot.Common.Models.Forums;

public interface IPublishable
{
    DateTime CreatedDate { get; set; }

    DateTime UpdatedDate { get; set; }

    DateTime InternalUpdatedDate { get; set; }

    DateTime? PublishDate { get; set; }

    bool IsPublished { get; set; }
}
