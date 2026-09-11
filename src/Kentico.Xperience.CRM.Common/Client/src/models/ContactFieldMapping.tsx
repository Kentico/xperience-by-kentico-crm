import Localization from '../localization/localization.json';

/**
 * Kind of value expression producing the value written to a single CRM field.
 * Mirrors ContactFieldValueKind on the server.
 */
export type ContactFieldValueKind =
  | 'SourceField'
  | 'Template'
  | 'Constant'
  | 'Coalesce'
  | 'Resolver';

export interface SelectOption {
  readonly value: string;
  readonly label: string;
}

/** Xperience contact field offered as a mapping source. */
export interface ContactSourceField {
  readonly name: string;
  readonly displayName: string;
  readonly dataType: string;
}

/** Named resolver offered for reference and coded contact fields. */
export interface MappingResolver {
  readonly name: string;
  readonly displayName: string;
  readonly defaultSourceField: string;
}

/** Writable CRM field offered as a mapping target. */
export interface CrmTargetField {
  readonly name: string;
  readonly displayName: string;
  readonly dataType: string;
  readonly isRequired: boolean;
  readonly isCustom: boolean;
  readonly maxLength: number | null;
  readonly options: SelectOption[];
}

/**
 * One row of the mapping grid. A row owns a CRM field and describes how its value is produced,
 * which is what lets several contact fields feed a single CRM field.
 */
export interface ContactFieldMappingRow {
  crmField: string;
  kind: ContactFieldValueKind;
  sourceField: string | null;
  template: string | null;
  constantValue: string | null;
  sourceFields: string[];
  resolver: string | null;
  enabled: boolean;
}

/** Everything that changes when the target entity type changes or the mapping is saved. */
export interface ContactFieldMappingData {
  readonly entityType: string;
  readonly crmFields: CrmTargetField[];
  readonly mappings: ContactFieldMappingRow[];
  readonly metadataIsLive: boolean;
  readonly metadataWarning: string | null;
  readonly hasConfiguration: boolean;
}

/** Properties supplied by ContactFieldMappingPage. */
export interface ContactFieldMappingProps {
  readonly crmName: string;
  readonly entityType: string;
  readonly entityTypes: SelectOption[];
  readonly sourceFields: ContactSourceField[];
  readonly resolvers: MappingResolver[];
  readonly crmFields: CrmTargetField[];
  readonly mappings: ContactFieldMappingRow[];
  readonly metadataIsLive: boolean;
  readonly metadataWarning: string | null;
  readonly hasConfiguration: boolean;
}

export interface LoadDefaultsResult {
  readonly mappings: ContactFieldMappingRow[];
}

export interface PreviewResult {
  readonly value: string | null;
  readonly sampleContact: string | null;
  readonly error: string | null;
}

/** Creates an empty row, used when the marketer adds a mapping. */
export const createEmptyRow = (): ContactFieldMappingRow => ({
  crmField: '',
  kind: 'SourceField',
  sourceField: null,
  template: null,
  constantValue: null,
  sourceFields: [],
  resolver: null,
  enabled: true,
});

/**
 * Describes how a row produces its value in one line. Shared by the review table and the collapsed
 * editor row so both read identically.
 */
export const describeMappingValue = (
  row: ContactFieldMappingRow,
  resolvers: MappingResolver[],
): string => {
  const notSet = Localization.integrations.crm.mapping.row.notSet;

  switch (row.kind) {
    case 'SourceField':
      return row.sourceField ?? notSet;

    case 'Template':
      return row.template ?? notSet;

    case 'Constant':
      return row.constantValue ? `"${row.constantValue}"` : notSet;

    case 'Coalesce':
      return row.sourceFields.length > 0
        ? row.sourceFields.join(' or ')
        : notSet;

    case 'Resolver': {
      const resolver = resolvers.find((item) => item.name === row.resolver);

      return resolver
        ? `${resolver.displayName} (${row.sourceField ?? resolver.defaultSourceField})`
        : notSet;
    }

    default:
      return notSet;
  }
};

/**
 * Returns the API name to show as a menu item's second line, or undefined when it would just repeat
 * the first. Labels frequently equal the API name - a contact field with no caption, or CRM metadata
 * with no label - and passing both unconditionally rendered every option twice.
 */
export const getSecondaryLabel = (
  primaryLabel: string,
  name: string,
): string | undefined => {
  const primary = primaryLabel.trim().toLowerCase();
  const secondary = name.trim().toLowerCase();

  // Case-insensitive so a PascalCase label over a lowercase API name counts as the same text, and
  // containment so a label that already quotes the name is not repeated either.
  return primary === secondary || primary.includes(secondary) ? undefined : name;
};

/** Resolves the label a CRM field should be shown under, falling back to its raw API name. */
export const describeCrmField = (
  crmField: string,
  crmFields: CrmTargetField[],
): string => {
  if (!crmField) {
    return Localization.integrations.crm.mapping.row.notSet;
  }

  return (
    crmFields.find((field) => field.name === crmField)?.displayName ?? crmField
  );
};
