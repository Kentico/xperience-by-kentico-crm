import {
  Box,
  Button,
  ButtonColor,
  ButtonSize,
  Checkbox,
  Cols,
  Column,
  Divider,
  DividerOrientation,
  Inline,
  Input,
  MenuItem,
  Paper,
  Row,
  RowWrap,
  Select,
  Spacing,
  Stack,
  Tag,
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
  describeMappingValue,
  getSecondaryLabel,
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

  return (
    <Paper>
      <Box spacing={Spacing.M}>
        <Stack spacing={Spacing.M}>
          {/* Row with Column children: only Column consumes the gutter variables Row sets, so this
              is the one place the Row spacing prop actually produces a gap. Both columns are a plain
              label plus input, which keeps their inputs on the same line. */}
          <Row spacing={Spacing.M} wrap={RowWrap.Wrap}>
            <Column cols={Cols.Col6}>
              {manual ? (
                <Input
                  label={Strings.columns.crmField}
                  value={row.crmField}
                  onChange={(event) =>
                    onChange({ ...row, crmField: event.target.value })
                  }
                />
              ) : (
                <Select
                  markAsRequired
                  label={Strings.columns.crmField}
                  value={row.crmField}
                  onChange={(value) =>
                    onChange({ ...row, crmField: value ?? '' })
                  }
                >
                  {crmFields.map((field) => (
                    <MenuItem
                      key={field.name}
                      value={field.name}
                      primaryLabel={field.displayName}
                      secondaryLabel={getSecondaryLabel(
                        field.displayName,
                        field.name,
                      )}
                    />
                  ))}
                </Select>
              )}
            </Column>
            <Column cols={Cols.Col6}>
              <Input
                readOnly
                label={Strings.columns.value}
                value={describeMappingValue(row, resolvers)}
                onClick={onToggleExpanded}
              />
            </Column>
          </Row>

          {/* Inline rather than Row: Inline wraps each child in a spaced Box, while Row would apply a
              negative margin and pull this toolbar up over the field labels above it. The data type
              lives here as a tag instead of as the select's explanation text, which used to collide
              with these controls. */}
          <Inline spacing={Spacing.S}>
            {selectedField && (
              <Tag
                label={
                  selectedField.isRequired
                    ? `${selectedField.dataType} - ${Strings.row.required}`
                    : selectedField.dataType
                }
              />
            )}
            <Checkbox
              label={Strings.row.enabled}
              checked={row.enabled}
              onChange={(_event, checked) =>
                onChange({ ...row, enabled: checked })
              }
            />
            <Button
              size={ButtonSize.S}
              color={ButtonColor.Secondary}
              label={
                manual ? Strings.row.selectCrmField : Strings.row.customField
              }
              onClick={() => setManual(!manual)}
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
          </Inline>

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
