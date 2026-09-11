import { Colors } from '@kentico/xperience-admin-components';
import React from 'react';

/**
 * Body copy for the custom pages.
 *
 * The component library has no text primitive, and a bare string dropped into a layout component
 * inherits whatever colour the surrounding admin styles happen to apply - which rendered the page
 * copy in the alert colour. Setting the colour from a design token keeps body text readable
 * regardless of context, without hardcoding a colour value.
 */
interface BodyTextProps {
  readonly children: React.ReactNode;
  /** Use 'subtle' for supporting copy such as descriptions and empty states. */
  readonly tone?: 'default' | 'subtle';
}

export const BodyText = ({
  children,
  tone = 'default',
}: BodyTextProps): JSX.Element => (
  <p
    style={{
      margin: 0,
      color:
        tone === 'subtle' ? Colors.TextLowEmphasis : Colors.TextDefaultOnLight,
      fontSize: 'var(--font-size-m)',
      lineHeight: 'var(--line-m)',
    }}
  >
    {children}
  </p>
);
