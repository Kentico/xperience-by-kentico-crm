import {
  CellType,
  ColumnContentType,
  type StringCell,
  Table,
  type TableColumn,
  type TableRow,
} from '@kentico/xperience-admin-components';
import React from 'react';
import Localization from '../localization/localization.json';
import {
  type ContactFieldMappingRow,
  type CrmTargetField,
  type MappingResolver,
  describeCrmField,
  describeMappingValue,
} from '../models/ContactFieldMapping';

const Strings = Localization.integrations.crm.mapping;

interface MappingOverviewTableProps {
  readonly mappings: ContactFieldMappingRow[];
  readonly crmFields: CrmTargetField[];
  readonly resolvers: MappingResolver[];
  /* eslint-disable @typescript-eslint/naming-convention */
  readonly onEditRow: (index: number) => void;
  /* eslint-enable @typescript-eslint/naming-convention */
}

const Columns = {
  CrmField: 'crmField',
  Value: 'value',
  Applied: 'applied',
};

const column = (
  name: string,
  caption: string,
  minWidth: number,
  maxWidth: number,
): TableColumn => ({
  name,
  caption,
  visible: true,
  minWidth,
  maxWidth,
  contentType: ColumnContentType.Text,
  sortable: false,
  searchable: false,
});

const stringCell = (
  columnName: string,
  value: string,
  tooltipText?: string,
): StringCell => ({
  type: CellType.String,
  columnName,
  value,
  tooltipText,
});

/**
 * Read-only view of every mapping for the selected target record, so a marketer can review the whole
 * configuration at a glance instead of expanding each row's editor. Selecting a row opens that row
 * in the editor.
 */
export const MappingOverviewTable = ({
  mappings,
  crmFields,
  resolvers,
  onEditRow,
}: MappingOverviewTableProps): JSX.Element => {
  const columns: TableColumn[] = [
    column(Columns.CrmField, Strings.overview.columns.crmField, 25, 35),
    column(Columns.Value, Strings.overview.columns.value, 35, 50),
    column(Columns.Applied, Strings.overview.columns.applied, 10, 15),
  ];

  const rows: TableRow[] = mappings.map((mapping, index) => ({
    identifier: index,
    // Rows that are kept but not applied are rendered muted by the table itself.
    disabled: !mapping.enabled,
    cells: [
      // The API name is the tooltip: the label alone is what a marketer reads, but the exact field
      // name is what they need when comparing against the CRM.
      stringCell(
        Columns.CrmField,
        describeCrmField(mapping.crmField, crmFields),
        mapping.crmField,
      ),
      stringCell(Columns.Value, describeMappingValue(mapping, resolvers)),
      stringCell(
        Columns.Applied,
        mapping.enabled
          ? Strings.overview.applied.yes
          : Strings.overview.applied.no,
      ),
    ],
  }));

  return (
    <Table
      columns={columns}
      rows={rows}
      onRowClick={(identifier) => onEditRow(Number(identifier))}
    />
  );
};
