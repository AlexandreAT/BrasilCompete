import { Flag } from 'lucide-react-native';

import { Container, fallbackIcon, FlagImage } from './style';
import type { CountryFlagProps } from './types';
import { useCountryFlag } from './useCountryFlag';

export function CountryFlag(props: CountryFlagProps) {
  const { accessibilityLabel, fallbackIconSize, flagSource, handleImageError, size } =
    useCountryFlag(props);

  return (
    <Container $size={size} accessibilityLabel={accessibilityLabel}>
      {flagSource ? (
        <FlagImage
          accessibilityIgnoresInvertColors
          onError={handleImageError}
          source={flagSource}
        />
      ) : (
        <Flag {...fallbackIcon} size={fallbackIconSize} />
      )}
    </Container>
  );
}
