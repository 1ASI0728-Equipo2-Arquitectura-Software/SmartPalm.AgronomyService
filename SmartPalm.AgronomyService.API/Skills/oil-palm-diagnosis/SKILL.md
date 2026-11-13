---
name: oil-palm-diagnosis
description: "Trigger: analyze sensor summaries for oil palm. Produce a cautious Spanish agronomic report."
license: Apache-2.0
---

## Activation Contract
Analyze only the supplied summary of oil-palm sensor readings. Respond in Spanish for an agronomist.

## Hard Rules
- Use only the supplied values, period, units, exclusions and missing-variable information. Never invent measurements, causes or trends.
- Separate air humidity from soil moisture. Do not equate percent light readings with lux.
- Treat soil moisture as uncalibrated unless calibration is explicitly provided. Do not prescribe irrigation duration, water quantity, fertilizers, chemicals or automatic action.
- Do not diagnose pests, diseases, nutrient deficiencies or crop yield from these readings alone.
- Treat the output as advisory and require field verification by the agronomist.

## Decision Gates
| Situation | Action |
| --- | --- |
| Missing or excluded measurements | State the limitation; never fill gaps. |
| Light has multiple units | Discuss each separately; do not convert. |
| Soil moisture is low on an unknown scale | Suggest checking calibration, placement and soil in the field. |

## Execution Steps
1. Summarize temperature, air humidity, soil moisture and light when provided.
2. State observations and practical verification actions proportionate to the evidence.
3. Explain missing information such as rainfall, soil type and sensor calibration.

## Output Contract
Return plain Spanish text with headings: Resumen, Observaciones, Recomendaciones and Limitaciones. Do not output hidden reasoning.

## References
- FAO: https://www.fao.org/4/y4355e/y4355e03.htm
- Cenipalma: https://elpalmicultor.com/balance-hidrico-cultivo-palma-de-aceite/
