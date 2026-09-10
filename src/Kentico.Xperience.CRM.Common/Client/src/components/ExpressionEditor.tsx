import {
  Box,
  Button,
  ButtonColor,
  ButtonSize,
  Input,
  MenuItem,
  Select,
  Spacing,
  Stack,
  Tag,
  TextArea,
} from '@kentico/xperience-admin-components';
import React, { useRef } from 'react';
import Localization from '../localization/localization.json';
import {
  type ContactFieldMappingRow,
  type ContactFieldValueKind,
  type ContactSourceField,
  type MappingResolver,
  type PreviewResult,
} from '../models/ContactFieldMapping';

const Strings = Localization.integrations.crm.mapping.expression;

interface ExpressionEditorProps {
  readonly row: ContactFieldMappingRow;
  readonly sourceFields: ContactSourceField[];
  readonly resolvers: MappingResolver[];
  readonly preview: PreviewResult | null;
  /* eslint-disable @typescript-eslint/naming-convention */
  readonly onChange: (row: ContactFieldMappingRow) => void;
  readonly onPreview: () => void;
  /* eslint-enable @typescript-eslint/naming-convention */
}

const kindOptions: Array<{ value: ContactFieldValueKind; label: string }> = [
  { value: 'SourceField', label: Strings.kinds.sourceField },
  { value: 'Template', label: Strings.kinds.template },
  { value: 'Constant', label: Strings.kinds.constant },
  { value: 'Coalesce', label: Strings.kinds.coalesce },
  { value: 'Resolver', label: Strings.kinds.resolver },
];

const isKind = (value: string): value is ContactFieldValueKind =>
  kindOptions.some((option) => option.value === value);

export const ExpressionEditor = ({
  row,
  sourceFields,
  resolvers,
  preview,
  onChange,
  onPreview,
}: ExpressionEditorProps): JSX.Element => {
  const templateRef = useRef<HTMLTextAreaElement>(null);

  const update = (changes: Partial<ContactFieldMappingRow>): void =>
    onChange({ ...row, ...changes });

  /**
   * Inserts a token at the caret so a marketer can build a template without typing braces.
   * Falls back to appending when the caret position is unknown.
   */
  const insertToken = (fieldName?: string): void => {
    if (!fieldName) {
      return;
    }

    const token = `{{${fieldName}}}`;
    const current = row.template ?? '';
    const textArea = templateRef.current;
    const caret = textArea ? textArea.selectionStart : current.length;

    update({
      template: `${current.slice(0, caret)}${token}${current.slice(caret)}`,
    });
  };

  const addFallbackField = (fieldName?: string): void => {
    if (!fieldName || row.sourceFields.includes(fieldName)) {
      return;
    }

    update({ sourceFields: [...row.sourceFields, fieldName] });
  };

  const removeFallbackField = (fieldName: string): void =>
    update({
      sourceFields: row.sourceFields.filter((field) => field !== fieldName),
    });

  const sourceFieldItems = sourceFields.map((field) => (
    <MenuItem
      key={field.name}
      value={field.name}
      primaryLabel={field.displayName}
      secondaryLabel={field.name}
    />
  ));

  return (
    <Box spacing={Spacing.M}>
      <Stack spacing={Spacing.M}>
        <Select
          label={Strings.kind}
          value={row.kind}
          onChange={(value) => {
            if (value && isKind(value)) {
              update({ kind: value });
            }
          }}
        >
          {kindOptions.map((option) => (
            <MenuItem
              key={option.value}
              value={option.value}
              primaryLabel={option.label}
            />
          ))}
        </Select>

        {row.kind === 'SourceField' && (
          <Select
            label={Strings.sourceField}
            value={row.sourceField ?? ''}
            onChange={(value) => update({ sourceField: value ?? null })}
          >
            {sourceFieldItems}
          </Select>
        )}

        {row.kind === 'Template' && (
          <Stack spacing={Spacing.S}>
            <TextArea
              label={Strings.template}
              explanationText={Strings.templateExplanation}
              value={row.template ?? ''}
              minRows={2}
              maxRows={6}
              textAreaRef={templateRef}
              onChange={(event) => update({ template: event.target.value })}
            />
            <Select
              label={Strings.insertField}
              value=""
              onChange={insertToken}
            >
              {sourceFieldItems}
            </Select>
          </Stack>
        )}

        {row.kind === 'Constant' && (
          <Input
            label={Strings.constant}
            value={row.constantValue ?? ''}
            onChange={(event) => update({ constantValue: event.target.value })}
          />
        )}

        {row.kind === 'Coalesce' && (
          <Stack spacing={Spacing.S}>
            {row.sourceFields.length > 0 && (
              <Stack spacing={Spacing.XS}>
                {row.sourceFields.map((fieldName, index) => (
                  <Tag
                    key={fieldName}
                    removable
                    label={`${index + 1}. ${fieldName}`}
                    onRemoveClick={() => removeFallbackField(fieldName)}
                  />
                ))}
              </Stack>
            )}
            <Select
              label={Strings.addFallback}
              explanationText={Strings.coalesceExplanation}
              value=""
              onChange={addFallbackField}
            >
              {sourceFieldItems}
            </Select>
          </Stack>
        )}

        {row.kind === 'Resolver' && (
          <Stack spacing={Spacing.S}>
            <Select
              label={Strings.resolver}
              value={row.resolver ?? ''}
              onChange={(value) => {
                const selected = resolvers.find(
                  (resolver) => resolver.name === value,
                );

                update({
                  resolver: value ?? null,
                  // Pre-fills the field the resolver reads, which is what a marketer wants
                  // in every case except an unusual custom field.
                  sourceField: selected
                    ? selected.defaultSourceField
                    : row.sourceField,
                });
              }}
            >
              {resolvers.map((resolver) => (
                <MenuItem
                  key={resolver.name}
                  value={resolver.name}
                  primaryLabel={resolver.displayName}
                />
              ))}
            </Select>
            <Select
              label={Strings.resolverSourceField}
              value={row.sourceField ?? ''}
              onChange={(value) => update({ sourceField: value ?? null })}
            >
              {sourceFieldItems}
            </Select>
          </Stack>
        )}

        <Stack spacing={Spacing.XS}>
          <Button
            size={ButtonSize.S}
            color={ButtonColor.Secondary}
            label={Strings.preview}
            onClick={onPreview}
          />
          {preview !== null && (
            <Input
              readOnly
              label={
                preview.sampleContact
                  ? `${Strings.previewFor} ${preview.sampleContact}`
                  : Strings.preview
              }
              invalid={Boolean(preview.error)}
              validationMessage={preview.error ?? undefined}
              value={preview.value ?? ''}
            />
          )}
        </Stack>
      </Stack>
    </Box>
  );
};
