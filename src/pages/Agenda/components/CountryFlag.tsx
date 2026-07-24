import { Flag } from 'lucide-react-native';
import { useEffect, useState } from 'react';
import { Image, StyleSheet, View } from 'react-native';

import { Theme } from '@/shared/themes/Theme';
import { getCountryFlagUrl } from '@/utils/getCountryFlagUrl';

type CountryFlagProps = {
  countryName: string;
  size?: number;
};

export function CountryFlag({ countryName, size = 64 }: CountryFlagProps) {
  const flagUrl = getCountryFlagUrl(countryName);
  const [hasImageError, setHasImageError] = useState(false);

  useEffect(() => {
    setHasImageError(false);
  }, [flagUrl]);

  const shouldShowFallback = !flagUrl || hasImageError;

  return (
    <View
      accessibilityLabel={
        shouldShowFallback
          ? `Bandeira de ${countryName} indisponível`
          : `Bandeira de ${countryName}`
      }
      className="items-center justify-center overflow-hidden rounded-full"
      style={[
        styles.container,
        {
          width: size,
          height: size,
        },
      ]}
    >
      {shouldShowFallback ? (
        <Flag
          color={Theme.colors.disabled}
          size={Math.round(size * 0.42)}
          strokeWidth={1.8}
        />
      ) : (
        <Image
          source={{ uri: flagUrl }}
          accessibilityIgnoresInvertColors
          onError={() => setHasImageError(true)}
          resizeMode="cover"
          style={styles.image}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: Theme.colors.flagFallback,
    borderColor: Theme.colors.softBorder,
    borderWidth: 1,
    elevation: 3,
    shadowColor: Theme.colors.darkNavy,
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.1,
    shadowRadius: 5,
  },
  image: {
    height: '100%',
    width: '100%',
  },
});
