import {
  Banner,
  Content,
  CountryHighlight,
  Description,
  HighlightLine,
  Overlay,
  Title,
} from './style';
import { HERO_BANNER_IMAGE } from './types';

export function HomeHeroBanner() {
  return (
    <Banner
      accessibilityLabel="Atleta brasileira comemorando em um estádio"
      source={HERO_BANNER_IMAGE}
    >
      <Overlay />

      <Content>
        <Title numberOfLines={3}>
          Acompanhe os{'\n'}próximos eventos{'\n'}do{' '}
          <CountryHighlight>Brasil</CountryHighlight>
        </Title>

        <HighlightLine />

        <Description numberOfLines={2}>
          Datas, horários e onde{'\n'}assistir em um só lugar.
        </Description>
      </Content>
    </Banner>
  );
}
