import {
  Box,
  Button,
  ButtonColor,
  ButtonSize,
  Checkbox,
  Divider,
  DividerOrientation,
  Input,
  MenuItem,
  Paper,
  Select,
  Spacing,
  Stack,
} from '@kentico/xperience-admin-components';
import React, { useState } from 'react';
import { ExpressionEditor } from './ExpressionEditor';
import Localization from '../localization/localization.json';
import {
  type ContactFieldMappingRow,
  type ContactSourceField,
  type CrmTargetField,
  type MappingResolver,
  type PreviewResult,
} from '../models/ContactFieldMapping';

const Strings = Localization.integrations.crm.mapping;

interface MappingRowProps {
  readonly row: ContactFieldMappingRow;
  readonly crmFields: CrmTargetField[];
  readonly sourceFields: ContactSourceField[];
  readonly resolvers: MappingResolver[];
  readonly expanded: boolean;
  readonly preview: PreviewResult | null;
  /* eslint-disable @typescript-eslint/naming-convention */
  readonly onChange: (row: ContactFieldMappingRow) => void;
  readonly onRemove: () => void;
  readonly onToggleExpanded: () => void;
  readonly onPreview: () => void;
  /* eslint-enable @typescript-eslint/naming-convention */
}

/**
 * Describes the row's value in one line, so the grid stays readable without expanding every row.
 */
const describeValue = (
  row: ContactFieldMappingRow,
  resolvers: MappingResolver[],
): string => {
  switch (row.kind) {
    case 'SourceField':
      return row.sourceField ?? Strings.row.notSet;

    case 'Template':
      return row.template ?? Strings.row.notSet;

    case 'Constant':
      return row.constantValue
        ? `"${row.constantValue}"`
        : Strings.row.notSet;

    case 'Coalesce':
      return row.sourceFields.length > 0
        ? row.sourceFields.join(' or ')
        : Strings.row.notSet;

    case 'Resolver': {
      const resolver = resolvers.find((item) => item.name === row.resolver);

      return resolver
        ? `${resolver.displayName} (${row.sourceField ?? resolver.defaultSourceField})`
        : Strings.row.notSet;
    }

    default:
      return Strings.row.notSet;
  }
};

export const MappingRow = ({
  row,
  crmFields,
  sourceFields,
  resolvers,
  expanded,
  preview,
  onChange,
  onRemove,
  onToggleExpanded,
  onPreview,
}: MappingRowProps): JSX.Element => {
  // A CRM field that the metadata does not list can still be valid - a custom field on a tenant whose
  // schema could not be retrieved. Such a row starts in manual mode so the name stays editable.
  const isKnownField = crmFields.some((field) => field.name === row.crmField);
  const [manual, setManual] = useState(row.crmField !== '' && !isKnownField);

  const selectedField = crmFields.find((field) => field.name === row.crmField);

  const toggleManual = (
    <Button
      borderless
      size={ButtonSize.XS}
      color={ButtonColor.Tertiary}
      label={manual ? Strings.row.selectCrmField : Strings.row.customField}
      onClick={() => setManual(!manual)}
    />
  );

  return (
    <Paper>
      <Box spacing={Spacing.M}>
        <Stack spacing={Spacing.S}>
          <Stack spacing={Spacing.S}>
            {manual ? (
              <Input
                label={Strings.columns.crmField}
                labelActionsElement={toggleManual}
                value={row.crmField}
                onChange={(event) =>
                  onChange({ ...row, crmField: event.target.value })
                }
              />
            ) : (
              <Select
                markAsRequired
                label={Strings.columns.crmField}
                labelActionsElement={toggleManual}
                value={row.crmField}
                explanationText={
                  selectedField
                    ? `${selectedField.dataType}${selectedField.isRequired ? ` - ${Strings.row.required}` : ''}`
                    : undefined
                }
                onChange={(value) => onChange({ ...row, crmField: value ?? '' })}
              >
                {crmFields.map((field) => (
                  <MenuItem
                    key={field.name}
                    value={field.name}
                    primaryLabel={field.displayName}
                    secondaryLabel={field.name}
                  />
                ))}
              </Select>
            )}

            <Input
              readOnly
              label={Strings.columns.value}
              value={describeValue(row, resolvers)}
              onClick={onToggleExpanded}
            />
          </Stack>

          <Stack spacing={Spacing.S}>
            <Checkbox
              label={Strings.row.enabled}
              checked={row.enabled}
              onChange={(_event, checked) => onChange({ ...row, enabled: checked })}
            />
            <Button
              size={ButtonSize.S}
              color={ButtonColor.Secondary}
              label={expanded ? Strings.row.collapse : Strings.row.expand}
              onClick={onToggleExpanded}
            />
            <Button
              destructive
              size={ButtonSize.S}
              color={ButtonColor.Secondary}
              label={Strings.row.remove}
              onClick={onRemove}
            />
          </Stack>

          {expanded && (
            <Stack spacing={Spacing.S}>
              <Divider orientation={DividerOrientation.Horizontal} />
              <ExpressionEditor
                row={row}
                sourceFields={sourceFields}
                resolvers={resolvers}
                preview={preview}
                onChange={onChange}
                onPreview={onPreview}
              />
            </Stack>
          )}
        </Stack>
      </Box>
    </Paper>
  );
};
