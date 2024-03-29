﻿using System.Xml.Serialization;

namespace StackerBot.Types;

[XmlRoot(ElementName = "feed", Namespace = "http://www.w3.org/2005/Atom")]
public class YouTubeFeed {
  [XmlElement(ElementName = "entry")]
  public List<YouTubeFeedEntry> YouTubeFeedEntries { get; set; } = [];
}

[XmlRoot(ElementName = "entry", Namespace = "http://www.w3.org/2005/Atom")]
public class YouTubeFeedEntry {
  [XmlElement(ElementName = "videoId", Namespace = "http://www.youtube.com/xml/schemas/2015")]
  public string? VideoId { get; set; }

  [XmlElement(ElementName = "channelId", Namespace = "http://www.youtube.com/xml/schemas/2015")]
  public string? ChannelId { get; set; }

  [XmlElement(ElementName = "published", Namespace = "http://www.w3.org/2005/Atom")]
  public string? Published { get; set; }
}
