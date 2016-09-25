using System;
using System.Collections.Generic;
using System.Linq;

namespace Modules.ThingOfTheDay
{
    class Content
    {
        /// <summary>
        /// If true, places this content into the priority queue.
        /// </summary>
        public bool Prioritised { get; set; }
        public uint Setter { get; set; }
        public string URL { get; set; }
        public long TimeStamp { get; set; }
    }

    class ContentMoveEvent
    {
        public Content Old { get; set; }
        public Content New { get; set; }

        public ContentMoveEvent(Content oldContent, Content newContent)
        {
            Old = oldContent;
            New = newContent;
        }
    }

    interface IContentQueue
    {
        event EventHandler<ContentMoveEvent> ContentMoved;
        event EventHandler<List<Content>> ContentsChanged;
        List<Content> Contents { get; }
        void Submit(Content content);
        Content Get();
        Content GetRandom();
        Content Move();
        void Sort();
        bool HasURL(string url);
    }

    class ContentQueue : IContentQueue
    {
        public event EventHandler<ContentMoveEvent> ContentMoved;
        public event EventHandler<List<Content>> ContentsChanged;

        List<Content> _contents;

        public List<Content> Contents { get { return _contents.ToList(); } }

        public ContentQueue(List<Content> contentData)
        {
            if (contentData == null)
            {
                _contents = new List<Content>();
            }
            else
            {
                _contents = contentData;
            }
        }

        public void Submit(Content content)
        {
            _contents.Add(content);
            content.Prioritised = true;
            content.TimeStamp = DateTime.UtcNow.ToBinary();
            ContentsChanged?.Invoke(this, _contents);
        }

        public Content Get()
        {
            if (_contents.Count != 0)
            {
                return _contents[0];
            }
            else
            {
                return null;
            }
        }

        public Content GetRandom()
        {
            var rnd = new Random().Next(0, _contents.Count);
            return _contents[rnd];
        }

        public Content Move()
        {
            Content oldContent = null;
            Content newContent = null;

            // Remove the first item.
            if (_contents.Count != 0)
            {
                oldContent = _contents[0];
                _contents.RemoveAt(0);
            }

            // Get the next item from the priority queue. 
            // If nothing exists in that queue, sort to form a new queue then get the first item.
            var next = _contents.FirstOrDefault(x => x.Prioritised == true);
            if (next == null && _contents.Count != 0)
            {
                Sort();
                next = _contents[0];
                newContent = next;
            }

            ContentMoved?.Invoke(this, new ContentMoveEvent(oldContent, newContent));
            ContentsChanged?.Invoke(this, _contents);

            return next;
        }

        public void Sort()
        {
            // Unflag all.
            _contents.ForEach(x => x.Prioritised = false);

            // Get one submission per author and add it to the priority queue.
            _contents
                .GroupBy(x => x.Setter)
                .Select(x => x.First())
                .OrderBy(x => x.TimeStamp)
                .ToList()
                .ForEach(x => x.Prioritised = true);

            ContentsChanged?.Invoke(this, _contents);
        }

        public bool HasURL(string url)
        {
            return _contents.Any(x => x.URL == url);
        }
    }
}
