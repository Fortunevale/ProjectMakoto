namespace ProjectMakoto.Entities.Translation;

public record TVar(string ValName, object Replacement, bool Sanitize = false)
{
}
