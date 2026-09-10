import { usePageCommand } from '@kentico/xperience-admin-base';
import {
  Box,
  Button,
  ButtonColor,
  ButtonSize,
  Headline,
  HeadlineSize,
  MenuItem,
  NotificationBarInfo,
  NotificationBarWarning,
  Select,
  Spacing,
  Stack,
} from '@kentico/xperience-admin-components';
import React, { useRef, useState } from 'react';
import { MappingRow } from '../components/MappingRow';
import Localization from '../localization/localization.json';
import {
  type ContactFieldMappingData,
  type ContactFieldMappingProps,
  type ContactFieldMappingRow,
  type CrmTargetField,
  type LoadDefaultsResult,
  type PreviewResult,
  createEmptyRow,
} from '../models/ContactFieldMapping';

const Strings = Localization.integrations.crm.mapping;

const Commands = {
  ChangeEntityType: 'ChangeEntityType',
  LoadDefaults: 'LoadDefaults',
  Preview: 'Preview',
  Save: 'Save',
};

interface EntityTypeArguments {
  readonly entityType: string;
}

interface SaveArguments {
  readonly entityType: string;
  readonly mappings: ContactFieldMappingRow[];
}

interface PreviewArguments {
  readonly mapping: ContactFieldMappingRow;
}

/**
 * Lists the required CRM fields no enabled row fills, which is the mistake most likely to make the
 * CRM reject a record.
 */
const getUnmappedRequiredFields = (
  crmFields: CrmTargetField[],
  rows: ContactFieldMappingRow[],
): string[] => {
  if (rows.length === 0) {
    return [];
  }

  const mapped = new Set(
    rows.filter((row) => row.enabled).map((row) => row.crmField.toLowerCase()),
  );

  return crmFields
    .filter((field) => field.isRequired && !mapped.has(field.name.toLowerCase()))
    .map((field) => field.displayName);
};

export const ContactFieldMappingTemplate = (
  props: ContactFieldMappingProps,
): JSX.Element => {
  const [entityType, setEntityType] = useState(props.entityType);
  const [crmFields, setCrmFields] = useState(props.crmFields);
  const [rows, setRows] = useState(props.mappings);
  const [metadataIsLive, setMetadataIsLive] = useState(props.metadataIsLive);
  const [hasConfiguration, setHasConfiguration] = useState(
    props.hasConfiguration,
  );
  const [expandedIndex, setExpandedIndex] = useState<number | null>(null);
  const [previews, setPreviews] = useState<Record<number, PreviewResult>>({});

  // The preview command result carries no row identity, so the requesting row is remembered here.
  // A ref rather than state, so the command callback never reads a stale value.
  const previewIndex = useRef<number | null>(null);

  const applyData = (data: ContactFieldMappingData | undefined): void => {
    if (!data) {
      return;
    }

    setEntityType(data.entityType);
    setCrmFields(data.crmFields);
    setRows(data.mappings);
    setMetadataIsLive(data.metadataIsLive);
    setHasConfiguration(data.hasConfiguration);
    setExpandedIndex(null);
    setPreviews({});
  };

  const { execute: changeEntityType } = usePageCommand<
    ContactFieldMappingData,
    EntityTypeArguments
  >(Commands.ChangeEntityType, { after: applyData });

  const { execute: save } = usePageCommand<
    ContactFieldMappingData,
    SaveArguments
  >(Commands.Save, { after: applyData });

  const { execute: loadDefaults } = usePageCommand<
    LoadDefaultsResult,
    EntityTypeArguments
  >(Commands.LoadDefaults, {
    after: (result) => {
      if (result) {
        setRows(result.mappings);
        setExpandedIndex(null);
        setPreviews({});
      }
    },
  });

  const { execute: preview } = usePageCommand<PreviewResult, PreviewArguments>(
    Commands.Preview,
    {
      after: (result) => {
        const index = previewIndex.current;

        if (result && index !== null) {
          setPreviews((current) => ({ ...current, [index]: result }));
        }
      },
    },
  );

  const updateRow = (index: number, row: ContactFieldMappingRow): void =>
    setRows((current) =>
      current.map((existing, position) =>
        position === index ? row : existing,
      ),
    );

  const removeRow = (index: number): void => {
    setRows((current) => current.filter((_row, position) => position !== index));
    setExpandedIndex(null);
  };

  const addRow = (): void =>
    setRows((current) => {
      setExpandedIndex(current.length);

      return [...current, createEmptyRow()];
    });

  const requestPreview = (index: number, row: ContactFieldMappingRow): void => {
    previewIndex.current = index;
    void preview({ mapping: row });
  };

  const unmappedRequiredFields = getUnmappedRequiredFields(crmFields, rows);

  return (
    <Box spacing={Spacing.XL}>
      <Stack spacing={Spacing.XL}>
        <Stack spacing={Spacing.S}>
          <Headline size={HeadlineSize.M}>
            {`${props.crmName} - ${Strings.headline}`}
          </Headline>
          <Box>{Strings.description}</Box>
        </Stack>

        <Select
          label={Strings.target}
          value={entityType}
          onChange={(value) => {
            if (value && value !== entityType) {
              void changeEntityType({ entityType: value });
            }
          }}
        >
          {props.entityTypes.map((option) => (
            <MenuItem
              key={option.value}
              value={option.value}
              primaryLabel={option.label}
            />
          ))}
        </Select>

        {!metadataIsLive && (
          <NotificationBarWarning>
            {Strings.fallbackMetadata}
          </NotificationBarWarning>
        )}

        <NotificationBarInfo>
          {hasConfiguration
            ? Strings.usingStoredMapping
            : Strings.usingCodeMapping}
        </NotificationBarInfo>

        {unmappedRequiredFields.length > 0 && (
          <NotificationBarWarning>
            {`${Strings.requiredUnmapped} ${unmappedRequiredFields.join(', ')}`}
          </NotificationBarWarning>
        )}

        {rows.length === 0 ? (
          <Box>{Strings.empty}</Box>
        ) : (
          <Stack spacing={Spacing.M}>
            {rows.map((row, index) => (
              <MappingRow
                key={`${index}-${row.crmField}`}
                row={row}
                crmFields={crmFields}
                sourceFields={props.sourceFields}
                resolvers={props.resolvers}
                expanded={expandedIndex === index}
                preview={previews[index] ?? null}
                onChange={(changed) => updateRow(index, changed)}
                onRemove={() => removeRow(index)}
                onToggleExpanded={() =>
                  setExpandedIndex(expandedIndex === index ? null : index)
                }
                onPreview={() => requestPreview(index, row)}
              />
            ))}
          </Stack>
        )}

        <Stack spacing={Spacing.S}>
          <Button
            size={ButtonSize.S}
            color={ButtonColor.Secondary}
            label={Strings.addMapping}
            onClick={addRow}
          />
          <Button
            size={ButtonSize.S}
            color={ButtonColor.Secondary}
            label={Strings.loadDefaults}
            onClick={() => void loadDefaults({ entityType })}
          />
          <Button
            size={ButtonSize.M}
            color={ButtonColor.Primary}
            label={Strings.save}
            onClick={() => void save({ entityType, mappings: rows })}
          />
        </Stack>
      </Stack>
    </Box>
  );
};
