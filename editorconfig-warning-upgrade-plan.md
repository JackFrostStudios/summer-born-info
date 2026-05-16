# EditorConfig Warning Upgrade Plan

Goal: raise every `suggestion`-level C# convention in `API/.editorconfig` to `warning`, fix the resulting repository violations one rule at a time, and commit after each completed step.

## Status

- [x] Step 1: `dotnet_style_predefined_type_for_locals_parameters_members`
- [x] Step 2: `dotnet_style_predefined_type_for_member_access`
- [x] Step 3: `dotnet_style_qualification_for_field`
- [x] Step 4: `dotnet_style_qualification_for_property`
- [x] Step 5: `csharp_preferred_modifier_order`
- [x] Step 6: `csharp_style_throw_expression`
- [ ] Step 7: `csharp_style_var_for_built_in_types`
- [ ] Step 8: `csharp_style_var_when_type_is_apparent`
- [ ] Step 9: `csharp_style_var_elsewhere`
- [ ] Step 10: `dotnet_diagnostic.CA1826`
- [ ] Step 11: `dotnet_diagnostic.CA1725`
- [ ] Step 12: `dotnet_naming_rule.constant_fields_should_be_pascal_case`
- [ ] Step 13: `dotnet_naming_rule.camel_case_for_private_internal_fields`
- [ ] Step 14: `dotnet_diagnostic.IDE0055`

## Working Notes

- Shared verification command: `dotnet build SummerBornInfo.sln`
- Mechanical fixes should prefer `dotnet format` with a targeted diagnostic where that keeps the commit scoped to one rule.
- Manual fixes should stay limited to the active rule before the commit is created.
- Update this file after each completed step so it remains the source of truth for progress.
