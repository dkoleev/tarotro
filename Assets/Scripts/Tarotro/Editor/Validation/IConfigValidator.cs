namespace Tarotro.Editor.Validation {
    public interface IConfigValidator {
        string Name { get; }
        ConfigValidationResult Validate();
    }
}
