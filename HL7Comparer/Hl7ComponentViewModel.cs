namespace HL7Comparer
{
    public class Hl7ComponentViewModel
    {
        private readonly Component _component;

        public Hl7ComponentViewModel(Component component)
        {
            _component = component;
        }

        public string Id => _component.Id;
        public string Value => _component.ToString();
    }
}