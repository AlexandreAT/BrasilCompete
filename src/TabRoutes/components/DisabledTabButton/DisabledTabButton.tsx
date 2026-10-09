import { Container } from './style';
import type { DisabledTabButtonProps } from './types';

export function DisabledTabButton({
  accessibilityLabel,
  children,
  style,
  testID,
}: DisabledTabButtonProps) {
  return (
    <Container
      accessibilityLabel={accessibilityLabel}
      accessibilityRole="button"
      accessibilityState={{ disabled: true }}
      style={style}
      testID={testID}
    >
      {children}
    </Container>
  );
}
