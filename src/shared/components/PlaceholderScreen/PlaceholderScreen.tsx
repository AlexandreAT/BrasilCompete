import { Container, Description, Title } from './style';
import type { PlaceholderScreenProps } from './types';

export function PlaceholderScreen({ description, title }: PlaceholderScreenProps) {
  return (
    <Container>
      <Title>{title}</Title>
      <Description>{description}</Description>
    </Container>
  );
}
