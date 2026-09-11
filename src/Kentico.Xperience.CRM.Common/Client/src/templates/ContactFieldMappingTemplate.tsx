import { usePageCommand } from '@kentico/xperience-admin-base';
import {
  Box,
  Button,
  ButtonColor,
  ButtonSize,
  Callout,
  CalloutPlacementType,
  CalloutType,
  Card,
  Cols,
  Column,
  Headline,
  HeadlineSize,
  Inline,
  LayoutAlignment,
  MenuItem,
  NameToggleButtons,
  Row,
  RowWrap,
  Select,
  Spacing,
  Stack,
  Tag,
} from '@kentico/xperience-admin-components';
import React, { useRef, useState } from 'react';
import { BodyText } from '../components/BodyText';
import { MappingOverviewTable } from '../components/MappingOverviewTable';
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

const Views = {
  Review: 'review',
  Edit: 'edit',
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
  // Review is the default: most visits are to check the configuration, not to change it.
  const [view, setView] = useState(Views.Review);

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
        // Defaults are a starting point, so drop the marketer into the editor to review them.
        setView(Views.Edit);
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

  const addRow = (): void => {
    setRows((current) => {
      setExpandedIndex(current.length);

      return [...current, createEmptyRow()];
    });
    setView(Views.Edit);
  };

  const editRow = (index: number): void => {
    setExpandedIndex(index);
    setView(Views.Edit);
  };

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
          <BodyText tone="subtle">{Strings.description}</BodyText>
        </Stack>

        <Card headline={Strings.sections.target}>
          <Stack spacing={Spacing.M}>
            {/* Column children so Row's gutter actually applies, and alignY End so the tag lines up
                with the select's input rather than floating level with its label. */}
            <Row
              spacing={Spacing.M}
              alignY={LayoutAlignment.End}
              wrap={RowWrap.Wrap}
            >
              <Column cols={Cols.Col6}>
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
              </Column>
              <Column>
                <Box spacingBottom={Spacing.S}>
                  <Tag
                    label={
                      hasConfiguration
                        ? Strings.status.storedMapping
                        : Strings.status.codeMapping
                    }
                  />
                </Box>
              </Column>
            </Row>

            {/* Callouts rather than notification bars: these are standing context for the page, not
                transient alerts, so they should not dominate it. */}
            <Callout
              type={CalloutType.QuickTip}
              placement={CalloutPlacementType.OnPaper}
            >
              {hasConfiguration
                ? Strings.usingStoredMapping
                : Strings.usingCodeMapping}
            </Callout>

            {!metadataIsLive && (
              <Callout
                type={CalloutType.FriendlyWarning}
                placement={CalloutPlacementType.OnPaper}
              >
                {Strings.fallbackMetadata}
              </Callout>
            )}
          </Stack>
        </Card>

        <Card
          headline={Strings.sections.mapping}
          footer={
            // Row only for the right alignment - with no spacing prop it adds no negative margin.
            // The gaps come from Inline, which spaces each child. The outer Inline uses a wider gap
            // so the primary action reads as separate from the two secondary ones.
            <Row alignX={LayoutAlignment.End}>
              <Inline spacing={Spacing.L}>
                <Inline spacing={Spacing.S}>
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
                </Inline>
                <Button
                  size={ButtonSize.M}
                  color={ButtonColor.Primary}
                  label={Strings.save}
                  onClick={() => void save({ entityType, mappings: rows })}
                />
              </Inline>
            </Row>
          }
        >
          <Stack spacing={Spacing.M}>
            {/* spacingBottom adds room between the toggle and the table below it. */}
            <Box spacingBottom={Spacing.S}>
              <Row alignX={LayoutAlignment.End}>
                <NameToggleButtons
                  orientation="horizontal"
                  selectedItemId={view}
                  items={[
                    { id: Views.Review, label: Strings.view.review },
                    { id: Views.Edit, label: Strings.view.edit },
                  ]}
                  onChange={(id) =>
                    setView(id === Views.Edit ? Views.Edit : Views.Review)
                  }
                />
              </Row>
            </Box>

            {unmappedRequiredFields.length > 0 && (
              <Callout
                type={CalloutType.FriendlyWarning}
                placement={CalloutPlacementType.OnPaper}
              >
                {`${Strings.requiredUnmapped} ${unmappedRequiredFields.join(', ')}`}
              </Callout>
            )}

            {rows.length === 0 && (
              <BodyText tone="subtle">{Strings.empty}</BodyText>
            )}

            {rows.length > 0 && view === Views.Review && (
              <Stack spacing={Spacing.S}>
                <MappingOverviewTable
                  mappings={rows}
                  crmFields={crmFields}
                  resolvers={props.resolvers}
                  onEditRow={editRow}
                />
                <BodyText tone="subtle">{Strings.overview.hint}</BodyText>
              </Stack>
            )}

            {rows.length > 0 && view === Views.Edit && (
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
          </Stack>
        </Card>
      </Stack>
    </Box>
  );
};
