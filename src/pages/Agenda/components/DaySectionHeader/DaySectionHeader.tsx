import { CalendarDays } from 'lucide-react-native';

import { Container, IconCircle, sectionIcon, Title } from './style';
import type { DaySectionHeaderProps } from './types';

export function DaySectionHeader({ title }: DaySectionHeaderProps) {
  return (
    <Container>
      <IconCircle>
        <CalendarDays {...sectionIcon} />
      </IconCircle>
      <Title>{title}</Title>
    </Container>
  );
}
