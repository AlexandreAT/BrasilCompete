import { ActiveIndicator, Container, Label } from './style';
import type { TabLabelProps } from './types';
import { useTabLabel } from './useTabLabel';

export function TabLabel({ focused, label }: TabLabelProps) {
  const { state } = useTabLabel({ focused });

  return (
    <Container>
      <Label $state={state}>{label}</Label>
      <ActiveIndicator $state={state} />
    </Container>
  );
}
