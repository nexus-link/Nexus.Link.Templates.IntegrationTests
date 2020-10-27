using Nexus.Link.Libraries.Core.Translation;

namespace Mocks.Helpers
{
    public class MockOrder
    {
        public string Id { get; set; }
        public int Items { get; set; }

        [TranslationConcept("order.status")]
        public string Status { get; set; }
    }
}