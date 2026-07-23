import { Text, View } from 'react-native';

export function Calendar() {
  return (
    <View className="flex-1 items-center justify-center bg-white px-6">
      <Text className="text-3xl font-bold">Calendário</Text>
      <Text className="mt-2 text-center">
        A visualização mensal dos eventos aparecerá aqui.
      </Text>
    </View>
  );
}
